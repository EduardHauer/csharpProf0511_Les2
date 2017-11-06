using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace csharpProf0511_Les2_WF
{
    class Game
    {

        static BufferedGraphicsContext context;
        static public BufferedGraphics buffer;

        static List<BaseObject> objs = new List<BaseObject>();

        static Random rand = new Random();


        #region Константы

        static int STARS_NUM = 100;
        static double GAME_SPEED = 1;
        static int LOW_STAR_SIZE = 1;
        static int HIGH_STAR_SIZE = 3;
        static int LOW_PLANET_SIZE = 100;
        static int HIGH_PLANET_SIZE = 200;
        static int LOW_ASTEROID_SIZE = 20;
        static int HIGH_ASTEROID_SIZE = 30;

        #endregion


        #region Свойства

        static public int Width { get; set; }
        static public int Height { get; set; }

        #endregion

        static public void Init(Form form)
        {
            // Графическое устройство для вывода графики
            Graphics g;

            // предоставляет доступ к главному буферу графического контекста для текущего приложения
            context = BufferedGraphicsManager.Current;
            g = form.CreateGraphics(); // Создаём объект - поверхность рисования и связываем его с формой

            // Запоминаем размеры формы
            Width = form.Width;
            Height = form.Height;

            // Связываем буфер в памяти с графическим объектом.
            // для того, чтобы рисовать в буфере
            buffer = context.Allocate(g, new Rectangle(0, 0, Width, Height));


            //Загружаем объекты
            Load();

            // Добавляем таймер для показа объектов
            Timer timer = new Timer();
            timer.Interval = 15;
            timer.Start();
            timer.Tick += Timer_Tick;

        }


        /// <summary>
        /// Обработчик таймера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Tick(object sender, EventArgs e)
        {
            Draw();
            Update();
            GenerateNewObjects();
        }


        /// <summary>
        /// Генерирует новые объекты каждый тик таймера.
        /// </summary>
        private static void GenerateNewObjects()
        {
            // В зависимости от скорости игры с вероятностью 2% формируем астероид
            if (rand.Next((int)(50 / GAME_SPEED)) == 0)
            {
                objs.Add(CreateRandomAsteroid());
            }
        }


        /// <summary>
        /// Отрисовка объектов
        /// </summary>
        static public void Draw()
        {
            buffer.Graphics.Clear(Color.Black);

            // Проходимся по списку объектов и отрисовываем каждый
            foreach (var obj in objs)
            {
                obj.Draw();
            }

            buffer.Render();
        }


        /// <summary>
        /// Загружаем первоначальные объекты
        /// </summary>
        static public void Load()
        {
            // Формируем нужное количество звезд
            for (int i = 0; i < STARS_NUM; i++)
            {
                objs.Add(CreateRandomStar());
            }

            // Создаем планету
            objs.Add(CreateRandomPlanet());
        }


        /// <summary>
        /// Обновляем состояние всех объектов
        /// </summary>
        static public void Update()
        {
            List<BaseObject> forRemove = new List<BaseObject>();

            // Проходимся по списку объектов
            foreach (var obj in objs)
            {
                // Если астероид или планета выходят за пределы экрана - добавляем в список на удаление
                if (!obj.Update() && (obj is Planet || obj is Asteroid))
                {
                    forRemove.Add(obj);
                }
            }

            // Проходимся по списку на удаление. Удаляем объекты из основного списка. 
            foreach (var obj in forRemove)
            {
                objs.Remove(obj);

                //Если была удалена планета, то формируем новую планету
                if (obj is Planet)
                {
                    objs.Add(CreateRandomPlanet());
                }
            }
        }


        /// <summary>
        /// Метод создания случайной звезды
        /// </summary>
        /// <returns></returns>
        static public Star CreateRandomStar()
        {
            int starSize = rand.Next(LOW_STAR_SIZE, HIGH_STAR_SIZE + 1);

            return new Star(
                        new Point(rand.Next(Width), rand.Next(Height)),
                        new Point(-starSize, 0),
                        new Size(starSize, starSize),
                        0.1 * GAME_SPEED
                    );
        }

        /// <summary>
        /// Метод создания случаной планеты
        /// </summary>
        /// <returns></returns>
        static public Planet CreateRandomPlanet()
        {
            int size = rand.Next(LOW_PLANET_SIZE, HIGH_PLANET_SIZE);

            return new Planet(
                        new Point(
                            Game.Width,
                            rand.Next(Game.Height) - (int)size / 2
                        ),
                        new Point(
                            -rand.Next(4, 6),
                            0
                        ),
                        new Size(
                            size,
                            size
                        ),
                        Color.FromArgb(rand.Next(byte.MaxValue), rand.Next(byte.MaxValue), rand.Next(byte.MaxValue)),
                        0.1 * GAME_SPEED
                    );
        }

        /// <summary>
        /// Метод создания случайного астероида
        /// </summary>
        /// <returns></returns>
        static public Asteroid CreateRandomAsteroid()
        {
            int size = rand.Next(LOW_ASTEROID_SIZE, HIGH_ASTEROID_SIZE);
            int x = rand.Next(Width * 2);
            int y;

            x = x > Width ? Width : x;
            y = x < Width ? rand.Next(2) * (Height + size) - size : rand.Next(Height);

            return new Asteroid(
                        new Point(
                            x,
                            y
                        ),
                        new Point(
                            -rand.Next(20, 30),
                            rand.Next(-5, 5)
                        ),
                        new Size(
                            size,
                            size
                        ),
                        (double)rand.Next(-10, 10) / 50,
                        0.1 * GAME_SPEED
                    );
        }
    }

    abstract class BaseObject
    {

        protected Point pos;
        protected Point dir;
        protected Size size;

        /// <summary>
        /// Модификатор скорости передвижения. Для того, чтобы сохранить стабильное количество кадров в секунду, 
        /// добавим модификатор скорости анимации, который будет уменьшать скорость движения, относительно частоты опроса.
        /// </summary>
        protected double speedMod = 1;

        /// <summary>
        /// Реальные дробные координаты. Координаты для точного вычисления с использованием модификатора скорости
        /// </summary>
        protected double realX;

        /// <summary>
        /// Реальные дробные координаты. Координаты для точного вычисления с использованием модификатора скорости
        /// </summary>
        protected double realY;

        public BaseObject(Point pos, Point dir, Size size)
        {
            this.pos = pos;
            this.dir = dir;
            this.size = size;
            realX = pos.X;
            realY = pos.Y;
        }

        public BaseObject(Point pos, Point dir, Size size, double speedMod) : this(pos, dir, size)
        {
            this.speedMod = speedMod;
        }

        virtual public void Draw()
        {
            Game.buffer.Graphics.DrawEllipse(Pens.White, pos.X, pos.Y, size.Width, size.Height);
        }

        /// <summary>
        /// Метод обновления координат объекта. При уходе объекта за пределы экрана вернет false, если объект остается на экране - true
        /// </summary>
        /// <returns></returns>
        abstract public bool Update();

    }

    class Asteroid: BaseObject
    {
        /// <summary>
        /// Угол поворота астероида в градусах. каждую итерацию направление движения астероида меняется на это значение. 
        /// Угол хранится в радианах, а задается через соответствующее свойство в градусах.
        /// </summary>
        protected double angle;
        
        /// <summary>
        /// Реальные координаты смещения. Нужны для рассчета нового смещения после поворота
        /// </summary>м
        protected double dirRealX;

        /// <summary>
        /// Реальные координаты смещения. Нужны для рассчета нового смещения после поворота
        /// </summary>
        protected double dirRealY;


        /// <summary>
        /// Свойство для задания и получения угла в градусах
        /// </summary>
        public double Angle {
            get
            {
                return angle * 180 / Math.PI;
            }
            set
            {
                angle = value * Math.PI / 180;
            }
        }

        public Asteroid(Point pos, Point dir, Size size, int angle) : this(pos, dir, size, angle, 1) { }
        /// <summary>
        /// Создание астероида
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <param name="size"></param>
        /// <param name="angle">Угол в градусах</param>
        /// <param name="speedMod"></param>
        public Asteroid(Point pos, Point dir, Size size, double angle, double speedMod) : base(pos, dir, size, speedMod)
        {
            this.Angle = angle;
            dirRealX = dir.X;
            dirRealY = dir.Y;
        }
        

        public override void Draw()
        {
            Game.buffer.Graphics.FillEllipse(new SolidBrush(Color.Brown), pos.X, pos.Y, size.Width, size.Height);
        }

        public override bool Update()
        {
            // Изменяем реальные координаты по формуле sin суммы и cos суммы и умножаем на гипотенузу, для нахождения стороны. 
            // Гипотенуза сокращается, поэтому она нам в формуле не нужна
            dirRealX = Math.Sin(angle) * dirRealY + Math.Cos(angle) * dirRealX;
            dirRealY = Math.Cos(angle) * dirRealY - Math.Sin(angle) * dirRealX;

            // Модифицируем координаты для отображения, они должны быть целыми числами.
            dir.X = (int) Math.Round(dirRealX);
            dir.Y = (int)Math.Round(dirRealY);

            realX += dir.X * speedMod;
            realY += dir.Y * speedMod;
            pos.X = (int)Math.Round(realX);
            pos.Y = (int)Math.Round(realY);

            if (pos.X < 0 - size.Width ||
                pos.X > Game.Width ||
                pos.Y < 0 - size.Height ||
                pos.Y > Game.Height) return false;

            return true;
        }

    }

    class Planet : BaseObject
    {

        protected Color color;

        public Planet(Point pos, Point dir, Size size, Color color) : this(pos, dir, size, color, 1) { }
        public Planet(Point pos, Point dir, Size size, Color color, double speedMod) : base(pos, dir, size, speedMod)
        {
            this.color = color;
        }

        public override void Draw()
        {
            Game.buffer.Graphics.FillEllipse(new SolidBrush(color), pos.X, pos.Y, size.Width, size.Height);
        }

        public override bool Update()
        {
            realX += dir.X * speedMod;
            realY += dir.Y * speedMod;
            pos.X = (int)Math.Round(realX);
            pos.Y = (int)Math.Round(realY);

            if (pos.X < 0 - size.Width ||
                pos.X > Game.Width ||
                pos.Y < 0 - size.Height ||
                pos.Y > Game.Height) return false;

            return true;
        }
    }

    class Star : BaseObject
    {

        public Star(Point pos, Point dir, Size size) : base(pos, dir, size) { }
        public Star(Point pos, Point dir, Size size, double speedMod) : base(pos, dir, size, speedMod) { }

        public override void Draw()
        {
            Game.buffer.Graphics.DrawLine(Pens.White, pos.X, pos.Y, pos.X + size.Width, pos.Y + size.Height);
            Game.buffer.Graphics.DrawLine(Pens.White, pos.X + size.Width, pos.Y, pos.X, pos.Y + size.Height);
        }

        public override bool Update()
        {
            bool isVisible = true;

            realX += dir.X * speedMod;
            realY += dir.Y * speedMod;
            pos.X = (int)Math.Round(realX);
            pos.Y = (int)Math.Round(realY);

            if (pos.X < 0 - size.Width ||
                pos.X > Game.Width ||
                pos.Y < 0 - size.Height ||
                pos.Y > Game.Height) isVisible = false;
            
            if (!isVisible)
            {
                realX = Game.Width;
            }
            return isVisible;
        }
    }

}