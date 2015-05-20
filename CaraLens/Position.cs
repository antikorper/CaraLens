using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaraParticles
{
    //Точка
    public class Point
    {
        private double _x;
        private double _y;
        private double _xCoordinate;
        private double _yCoordinate;

        public double x
        {
            get { return _x; }
            set
            {
                _x = value;
                _xCoordinate = Math.Round(78.0 + value / (111320.0 * Math.Cos(_yCoordinate * Math.PI / 180.0)), 4);
            }
        }

        public double y
        {
            get { return _y; }
            set
            {
                _y = value;
                _yCoordinate = Math.Round(74.0 + value / 111135.0, 4);
            }
        }

        //Долгота
        public double xCoordinate
        {
            get { return _xCoordinate; }
            set { _xCoordinate = value; }
        }

        //Широта
        public double yCoordinate
        {
            get { return _yCoordinate; }
            set { _yCoordinate = value; }
        }
    }

    //Положение точки
    public class Position : Point
    {
        public DateTime t;

        //Запись положения точки
        public void logPosition(StringBuilder c, StringBuilder k)
        {
            var csvLine = string.Format("{0};{1};{2}\n", yCoordinate, xCoordinate, t);
            var kmlLine = string.Format("{0},{1},0\n", xCoordinate.ToString("G", CultureInfo.InvariantCulture), yCoordinate.ToString("G", CultureInfo.InvariantCulture));
            Console.WriteLine(csvLine);
            c.Append(csvLine);
            k.Append(kmlLine);
        }

        //Изменение положения точки за время deltaT под действием ветра v
        public void changePosition(Wind v, int deltaT)
        {
            double Vs = 0;
            double Fis = 0;
            switch (Mover.calculationMethod)
            {
                case 1:
                    // Первый случай
                    Vs = Math.Pow(2, 0.5) * v.Utr / 0.4;
                    Fis = -Math.PI / 4.0;
                    break;
                case 2:
                    // Второй случай
                    Vs = v.Utr / 0.4 * Math.Pow(Math.Pow(Math.PI / 2.0, 2) + Math.Pow((-1.15 + Math.Log((0.4 * v.Utr * 30.0) / (0.05 * 0.00014), Math.E)), 2), 0.5);
                    Fis = -10.0 * Math.PI / 180.0;
                    break;
                case 3:
                    // Третий случай
                    Vs = 1.5 * v.Utr / 0.4 * Math.Pow(Math.Pow(Math.PI / 2.0, 2) + Math.Pow((-1.15 + Math.Log((0.4 * v.Utr * 30.0) / (0.05 * 0.00014), Math.E)), 2), 0.5);
                    Fis = -50.0 * Math.PI / 180.0;
                    break;
            }

            this.y += deltaT * Vs * Math.Sin(v.Fi + Fis);
            this.x += deltaT * Vs * Math.Cos(v.Fi + Fis);
            this.t = t.AddSeconds(deltaT);
        }
    }

    //Вектор
    public class Wind
    {
        private double _uComponent;
        private double _vComponent;
        private double? _Fi;
        private double? _Utr;

        public double uComponent
        {
            get { return _uComponent; }
            set { _uComponent = value; }
        }

        public double vComponent
        {
            get { return _vComponent; }
            set { _vComponent = value; }
        }

        //Модуль вектора
        public double W
        {
            get { return Math.Pow((Math.Pow(_uComponent, 2) + Math.Pow(_vComponent, 2)), 0.5); }
        }

        //Угол поворота вектора
        public double Fi
        {
            get { return _Fi != null ? _Fi.Value : Math.Atan2(_vComponent, _uComponent); }
            set { _Fi = value; }
        }

        public double Cd
        {
            get { return W <= 10.0 ? 1.14 / 1000.0 : (0.49 + 0.065 * W) / 1000.0; }
        }

        //Напряжение трения ветра
        public double Utr
        {
            get { return _Utr != null ? _Utr.Value : Math.Pow(1.28 * Cd / 1000.0, 0.5) * W; }
            set { _Utr = value; }
        }
    }

    public static class Mover
    {
        // Метод расчета
        public static int calculationMethod;

        // Способ интерполяции
        public static int interpolationMethod;

        //Время пока можно двигаться
        private static DateTime _tBorder = new DateTime(2007, 11, 1, 0, 0, 0);

        //Сведения о массиве
        public static DataTable windData;
        public static double arrayMaxX;
        public static double arrayMinX;
        public static double arrayMaxY;
        public static double arrayMinY;

        //Массивы для формирования выходных файлов
        public static StringBuilder csv = new StringBuilder();
        public static StringBuilder kml = new StringBuilder();

        //Метод переопределения положения точки
        public static Position getPosition(Position point)
        {
            point.logPosition(csv, kml);

            Position nextStepPoint = doStep(point);

            if (nextStepPoint == null ||
                nextStepPoint.xCoordinate > arrayMaxX ||
                nextStepPoint.yCoordinate > arrayMaxY ||
                nextStepPoint.xCoordinate < arrayMinX ||
                nextStepPoint.yCoordinate < arrayMinY ||
                nextStepPoint.t > _tBorder)
            {
                return point;
            }

            return getPosition(nextStepPoint);
        }

        //Шаг
        private static Position doStep(Position point)
        {
            Wind wind = getWind(point);
            if (wind != null)
            {
                point.changePosition(wind, 6 * 3600);
                return point;
            }
            else return null;
        }

        //Определяем ветр в точке
        private static Wind getWind(Position point)
        {
            Wind v = interpolate(point);
            Wind W = null;
            if (v != null)
            {
                switch (interpolationMethod)
                {
                    case 1:
                        W = new Wind
                        {
                            uComponent = v.uComponent,
                            vComponent = v.vComponent
                        };
                        break;

                    case 2:
                        W = new Wind
                        {
                            Utr = v.Utr,
                            Fi = v.Fi
                        };
                        break;
                }
                return W;
            }
            else return null;
        }

        //Загрузка массива из файла
        public static void readWindData(string filePath)
        {
            DataTable tbl = new DataTable();

            for (int col = 0; col < 8; col++)
                tbl.Columns.Add(new DataColumn("Column" + (col + 1).ToString()));

            string[] lines = System.IO.File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                var cols = line.Split(null);

                DataRow dr = tbl.NewRow();
                int i = 0;
                for (int cIndex = 0; cIndex < cols.Count(); cIndex++)
                {
                    if (!String.IsNullOrEmpty(cols[cIndex]))
                    {
                        dr[i] = cols[cIndex];
                        i++;
                    }
                }

                tbl.Rows.Add(dr);
            }

            windData = tbl;
            arrayMaxX = windData.AsEnumerable().Select(row => Convert.ToDouble(row.Field<string>(5), CultureInfo.InvariantCulture)).ToList().Max();
            arrayMinX = windData.AsEnumerable().Select(row => Convert.ToDouble(row.Field<string>(5), CultureInfo.InvariantCulture)).ToList().Min();
            arrayMaxY = windData.AsEnumerable().Select(row => Convert.ToDouble(row.Field<string>(4), CultureInfo.InvariantCulture)).ToList().Max();
            arrayMinY = windData.AsEnumerable().Select(row => Convert.ToDouble(row.Field<string>(4), CultureInfo.InvariantCulture)).ToList().Min();
        }

        //Поиск значений компонетов ветра в узлах из загруженного массива 
        private static Wind getWindComponentsInNode(Position point)
        {
            return new Wind
            {
                uComponent = Convert.ToDouble(windData.AsEnumerable().FirstOrDefault(r => Math.Round(Convert.ToDouble(r.Field<string>(5), CultureInfo.InvariantCulture), 4) == point.xCoordinate
                                                        && Math.Round(Convert.ToDouble(r.Field<string>(4), CultureInfo.InvariantCulture), 4) == point.yCoordinate
                                                        && Convert.ToInt32(r.Field<string>(0), CultureInfo.InvariantCulture) == point.t.Year
                                                        && Convert.ToInt32(r.Field<string>(1), CultureInfo.InvariantCulture) == point.t.Month
                                                        && Convert.ToInt32(r.Field<string>(2), CultureInfo.InvariantCulture) == point.t.Day
                                                        && Convert.ToInt32(r.Field<string>(3), CultureInfo.InvariantCulture) == point.t.Hour).Field<string>(6), CultureInfo.InvariantCulture),

                vComponent = Convert.ToDouble(windData.AsEnumerable().FirstOrDefault(r => Math.Round(Convert.ToDouble(r.Field<string>(5), CultureInfo.InvariantCulture), 4) == point.xCoordinate
                                                        && Math.Round(Convert.ToDouble(r.Field<string>(4), CultureInfo.InvariantCulture), 4) == point.yCoordinate
                                                        && Convert.ToInt32(r.Field<string>(0), CultureInfo.InvariantCulture) == point.t.Year
                                                        && Convert.ToInt32(r.Field<string>(1), CultureInfo.InvariantCulture) == point.t.Month
                                                        && Convert.ToInt32(r.Field<string>(2), CultureInfo.InvariantCulture) == point.t.Day
                                                        && Convert.ToInt32(r.Field<string>(3), CultureInfo.InvariantCulture) == point.t.Hour).Field<string>(7), CultureInfo.InvariantCulture)
            };
        }

        //Вычисление интерполированного значения
        private static Wind interpolate(Position point)
        {
            //Узлы ячейки, куда попала точка
            double xMin = 0;
            double xMax = 0;
            double yMin = 0;
            double yMax = 0;
            Wind v = null;

            //Для Y мало точек и не равномерная сетка, поэтому перебор
            if (point.yCoordinate > 75.2351 && point.yCoordinate <= 77.1394) { yMin = 75.2351; yMax = 77.1394; }
            else if (point.yCoordinate > 73.3307 && point.yCoordinate <= 75.2351) { yMin = 73.3307; yMax = 75.2351; }
            else if (point.yCoordinate > 71.4262 && point.yCoordinate <= 73.3307) { yMin = 71.4262; yMax = 73.3307; }
            else if (point.yCoordinate >= 69.5217 && point.yCoordinate <= 71.4262) { yMin = 69.5217; yMax = 71.4262; };

            //Для X сетка равномерная, можно посчитать по формуле
            if (point.xCoordinate >= 50.625 && point.xCoordinate <= 95.625)
            {
                xMin = Math.Floor(point.xCoordinate / 1.875) * 1.875;
                xMax = Math.Ceiling(point.xCoordinate / 1.875) * 1.875;
            };

            //Если удалось определить все узлы ячейки, то все хорошо и можно интерполировать
            if (xMin * xMax * yMin * yMax != 0)
            {
                Wind v1 = getWindComponentsInNode(new Position { xCoordinate = xMin, yCoordinate = yMin, t = point.t });
                Wind v2 = getWindComponentsInNode(new Position { xCoordinate = xMax, yCoordinate = yMin, t = point.t });
                Wind v3 = getWindComponentsInNode(new Position { xCoordinate = xMin, yCoordinate = yMax, t = point.t });
                Wind v4 = getWindComponentsInNode(new Position { xCoordinate = xMax, yCoordinate = yMax, t = point.t });

                switch (interpolationMethod)
                {
                    case 1:
                        v = new Wind
                        {
                            uComponent = v1.uComponent * (xMax - point.xCoordinate) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v2.uComponent * (point.xCoordinate - xMin) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v3.uComponent * (xMax - point.xCoordinate) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v4.uComponent * (point.xCoordinate - xMin) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)),

                            vComponent = v1.vComponent * (xMax - point.xCoordinate) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v2.vComponent * (point.xCoordinate - xMin) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v3.vComponent * (xMax - point.xCoordinate) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                         v4.vComponent * (point.xCoordinate - xMin) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin))
                        };
                        break;

                    case 2:
                        v = new Wind
                        {
                            Utr = v1.Utr * (xMax - point.xCoordinate) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                  v2.Utr * (point.xCoordinate - xMin) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                  v3.Utr * (xMax - point.xCoordinate) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                  v4.Utr * (point.xCoordinate - xMin) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)),

                            Fi = v1.Fi * (xMax - point.xCoordinate) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                 v2.Fi * (point.xCoordinate - xMin) * (yMax - point.yCoordinate) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                 v3.Fi * (xMax - point.xCoordinate) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin)) +
                                 v4.Fi * (point.xCoordinate - xMin) * (point.yCoordinate - yMin) * (1.0 / (xMax - xMin)) * (1.0 / (yMax - yMin))
                        };
                        break;
                }
                return v;
            }
            else return null;
        }
    }
}