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
                x = 0.0,
                y = 0.0,
                t = new DateTime(2007, 6, 1, 0, 0, 0)
            };

            //string dir = "C:\\Users\\lyzhkovda\\!Work items\\DEV\\CaraParticles\\";
            string dir = "c:\\Users\\Dmitry\\!Аспирантура\\Caradag\\";
            string kmlHead = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                             "<kml xmlns=\"http://earth.google.com/kml/2.0\">" +
                            "<Document>\n<Placemark>\n<LineString>\n<Style>\n<LineStyle>\n<color>#00FF00</color>\n<width>3</width>\n</LineStyle>\n</Style>\n<coordinates>";

            string kmlTale = "</coordinates>\n</LineString>\n" +
                             "</Placemark>\n</Document>\n</kml>";

            Mover.readWindData(dir + "uv2007MayNov.dat");
            Mover.kml.Append(kmlHead);
            Console.WriteLine(string.Format("First point: {0}; {1}; {2}", firstPoint.xCoordinate, firstPoint.yCoordinate, firstPoint.t));

            //Основной метод
            Position lastPoint = Mover.getPosition(firstPoint);

            Console.WriteLine(string.Format("Last point: {0}; {1}; {2}", lastPoint.xCoordinate, lastPoint.yCoordinate, lastPoint.t));
            Mover.kml.Append(kmlTale);

            //Формирование файлов
            File.WriteAllText(dir + "output.kml", Mover.kml.ToString());
            File.WriteAllText(dir + "output.csv", Mover.csv.ToString());

            Console.ReadKey();
        }
    }
}