using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace csharpProf0511_Les2_Homework
{
    abstract class Worker
    {
        public double payment;

        public Worker(double payment)
        {
            this.payment = payment;
        }

        /// <summary>
        /// Среднемесячная заработная плата
        /// </summary>
        /// <returns>Среднемесячная заработная плата</returns>
        abstract public double AverageMonthlyPay();

        public static void RandomWorker(ref Worker w)
        {
            Random r = new Random();

            switch(r.Next(1))
            {
                case 0:
                    w = new WorkerA(r.Next(100));
                    break;
                case 1:
                    w = new WorkerB(r.Next(20000));
                    break;
                default:
                    w = new WorkerA(0);
                    break;
            }
        }

        public override string ToString()
        {
            return $"{payment}";
        }
    }

    class WorkerA : Worker, IComparable
    {
        public WorkerA(double payment) : base(payment)
        {
        }

        public WorkerA(Worker w) : base(w.payment)
        {

        }

        /// <summary>
        /// Среднемесячная заработная плата
        /// </summary>
        /// <returns>Среднемесячная заработная плата</returns>
        public override double AverageMonthlyPay()
        {
            return 20.8 * 8 * payment;
        }

        public int CompareTo(object obj)
        {
            if (payment > (obj as Worker).payment) return -1;
            else if (payment < (obj as Worker).payment) return 1;
            return 0;
        }
    }

    class WorkerB : Worker, IComparable
    {
        public WorkerB(double payment) : base(payment)
        {
        }

        public WorkerB(Worker w) : base(w.payment)
        {

        }

        /// <summary>
        /// Среднемесячная заработная плата
        /// </summary>
        /// <returns>Среднемесячная заработная плата</returns>
        public override double AverageMonthlyPay()
        {
            return payment;
        }

        public int CompareTo(object obj)
        {
            if (this.payment > (obj as Worker).payment) return -1;
            else if (this.payment < (obj as Worker).payment) return 1;
            return 0;
        }
    }

    class WorkerList : IEnumerable
    {
        // массив сотрудников
        public Worker[] wList;
        
        public WorkerList(params Worker[] wList)
        {
            this.wList = wList;
        }

        // вывод данных
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < wList.Length; i++)
                yield return wList[i];
        }
    }
}
