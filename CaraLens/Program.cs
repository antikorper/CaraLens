using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaraParticles
{
    class Program
    {
        static void Main()
        {
            //Начальное положение точки
            Position firstPoint = new Position
            {
                y = 0.0,
                x = 0.0,
                t = new DateTime(2007, 6, 1, 0, 0, 0)
            };

            //string dir = "C:\\Users\\lyzhkovda\\!Work items\\DEV\\CaraParticles\\";
            string dir = "c:\\Users\\Dmitry\\!Аспирантура\\Caradag\\";
            Mover.readWindData(dir + "uv2007MayNov.dat");
 
            Console.WriteLine(string.Format("First point: {0}; {1}; {2}", firstPoint.yCoordinate, firstPoint.xCoordinate, firstPoint.t));

            //Выбираем расчетный метод и способ интерполяции
            Mover.calculationMethod = 1;
            Mover.interpolationMethod = 2;

            #region KMLsettings
            string kmlHead = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                             "<kml xmlns=\"http://earth.google.com/kml/2.0\">" +
                            "<Document>\n<Placemark>\n<LineString>\n<coordinates>";
            Mover.kml.Append(kmlHead);
            string lineColor = "";
            switch (Mover.calculationMethod)
            {
                case 1:
                    lineColor = "ff000000";
                    break;
                case 2:
                    lineColor = "50F00014";
                    break;
                case 3:
                    lineColor = "501400B4";
                    break;
            }

            string kmlTale = string.Format(" </coordinates>\n</LineString>\n<Style>\n<LineStyle>\n<color>{0}</color>", lineColor) +
                            "\n<width>4</width></LineStyle>\n</Style>\n</Placemark>\n</Document>\n</kml>";
            #endregion

            //Основной метод
            Position lastPoint = Mover.getPosition(firstPoint);

            Console.WriteLine(string.Format("Last point: {0}; {1}; {2}", lastPoint.yCoordinate, lastPoint.xCoordinate, lastPoint.t));
            Mover.kml.Append(kmlTale);

            //Формирование файлов
            File.WriteAllText(string.Format("{0}output_{1}_{2}.kml", dir, Mover.calculationMethod, Mover.interpolationMethod), Mover.kml.ToString());
            File.WriteAllText(string.Format("{0}output_{1}_{2}.csv", dir, Mover.calculationMethod, Mover.interpolationMethod), Mover.csv.ToString());

            Console.ReadKey();
        }
    }
}