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
            string kmlHead = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                             "<kml xmlns=\"http://earth.google.com/kml/2.0\">" +
                            "<Document>\n<Placemark>\n<LineString>\n<coordinates>";

            string kmlTale = " </coordinates>\n</LineString>\n<Style>\n<LineStyle>\n<color>ff000000</color>" +
                            "\n<width>5</width></LineStyle>\n</Style>\n</Placemark>\n</Document>\n</kml>";

            Mover.readWindData(dir + "uv2007MayNov.dat");
            Mover.kml.Append(kmlHead);
            Console.WriteLine(string.Format("First point: {0}; {1}; {2}", firstPoint.yCoordinate, firstPoint.xCoordinate, firstPoint.t));

            //Выбираем расчетный метод и способ интерполяции
            Mover.calculationMethod = 3;
            Mover.interpolationMethod = 2;

            //Основной метод
            Position lastPoint = Mover.getPosition(firstPoint);

            Console.WriteLine(string.Format("Last point: {0}; {1}; {2}", lastPoint.yCoordinate, lastPoint.xCoordinate, lastPoint.t));
            Mover.kml.Append(kmlTale);

            //Формирование файлов
            File.WriteAllText(dir + "output_" + Mover.calculationMethod.ToString() + "_" + Mover.interpolationMethod.ToString() + ".kml", Mover.kml.ToString());
            File.WriteAllText(dir + "output_" + Mover.calculationMethod.ToString() + "_" + Mover.interpolationMethod.ToString() + ".csv", Mover.csv.ToString());

            Console.ReadKey();
        }
    }
}