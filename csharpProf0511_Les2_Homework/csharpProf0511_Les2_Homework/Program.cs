using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
*    Eduard Hauer | www.vk.com/edhauer | eduard.hauer.genadiy@gmail.com
*/

namespace csharpProf0511_Les2_Homework
{
    class Program
    {
        static void Main(string[] args)
        {
            // !Тут баг с заполнением массива (запускай через CTRL + F5)!

            // Заполнение массива wList
            WorkerList wList = new WorkerList(new Worker[10]);
            for(int i = 0; i < 10; i++)
            {
                Worker w = new WorkerA(0);
                Worker.RandomWorker(ref w);
                wList.wList[i] = new WorkerA(w);
            }


            // Считования из wList.
            foreach(var item in wList)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }
    }
}
