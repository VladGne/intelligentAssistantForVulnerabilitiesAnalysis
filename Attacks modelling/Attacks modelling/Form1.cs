using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace Attacks_modelling
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();         
        }

        List<string[]> attackInfo = new List<string[]>();
        List<Element> Connect = new List<Element>();
        public int _id = 0;
        public List<Element> elements = new List<Element>();   
        int[] _ids = new int[2] {0, 0};                           
        public Stopwatch stopWatch = new Stopwatch();
        public Image addImg = null;

        private void Form1_Load(object sender, EventArgs e)
        {

            for (int i = 0; i < 6; i++)
            {
                dataGridView.Rows.Add();             
            }

            dataGridView[0, 0].Value = "Код";
            dataGridView[0, 1].Value = "Название";
            dataGridView[0, 2].Value = "ПО";
            dataGridView[0, 3].Value = "Рейтинг защищенности";
            dataGridView[0, 4].Value = "Количество";
            dataGridView[0, 5].Value = "Значимость";
        }

        public class Element: PictureBox
        {             
            public int id;                                      
            public decimal value;                                                           
            public bool Wait = false;
            public int graf;           
            public string Type;
            private bool B = false;                             
            public decimal risk;                                
            public decimal protectionRank;                      
            public string OS;
            public int count;
            public string name;
            public List<Element> Next = new List<Element>();
            public decimal maxRisk;
            public List<int> Connections = new List<int>();
            public string Results;                                 
            public Point UP = new Point();
            public Point DOWN = new Point();

            override public String ToString()
            {
                return Environment.NewLine + String.Format("Элемент [id - {0}, value - {1}, risk - {2}, connections - {3}]", this.id, this.value, this.risk, string.Join(",", Connections.ToArray()));
            }
            public Element()
            {
                MouseDown += new System.Windows.Forms.MouseEventHandler(this.img_MouseDown);
                MouseUp += new System.Windows.Forms.MouseEventHandler(this.img_MouseUp);
                MouseMove += new System.Windows.Forms.MouseEventHandler(this.img_MouseMove);
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            }          

            private void img_MouseDown(object sender, MouseEventArgs e)
            {
                B = true;
            }

            private void img_MouseUp(object sender, MouseEventArgs e)
            {
                B = false;
            }

            private void img_MouseMove(object sender, MouseEventArgs e)
            {
                if (!B)
                    return;
                Location = new Point(Location.X + e.X - Width, Location.Y + e.Y - Height);
                UP = new Point(Location.X + Width / 2, Location.Y);
                DOWN = new Point(Location.X + Width / 2, Location.Y + Height);
                Graf();
            }

            public delegate void print();
            public event print _print;

            protected virtual void Graf()
            {
                if (_print != null)
                    _print();
            }
          
            public void UD()
            {
                UP = new Point(Location.X + Width / 2, Location.Y);
                DOWN = new Point(Location.X + Width / 2, Location.Y + Height);
            }          
            
         
        }
       
        private void img_click(object sender, EventArgs e)  // Кликнули на элемент
        {
            var cur_elem = (Element)sender;
            int click_element_id = cur_elem.id;

            _ids[1] = _ids[0];
            _ids[0] = click_element_id;

            ShowParams(cur_elem);
        }

        private void ShowParams(Element element)
        {            
            dataGridView[1, 0].Value = element.id; 
            dataGridView[1, 1].Value = element.name;
            dataGridView[1, 2].Value = element.OS;
            dataGridView[1, 3].Value = element.protectionRank;
            dataGridView[1, 4].Value = element.count;
            dataGridView[1, 5].Value = element.value;
        }

        private void PCBtn_Click(object sender, EventArgs e)  // Создаём элемент
        {
            _id++;
            CreateElement(_id, "ПК", "ОС", 1, 1, 1, "50, 10", "PC");
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= elements.Count - 1; i++)
                elements[i].Dispose();           
            elements.Clear();
            _id = 0;
            panel1.Refresh();
        }

        private void SwitchBtn_Click(object sender, EventArgs e)
        {         
            _id++;
            CreateElement(_id, "Коммутатор", "ОС", 1, 1, 1, "55, 10", "Switch");
        }

        // проводим линию между элементами
        void print()
        {         
            panel1.Refresh();
            Pen pen = new Pen(Color.Black);

            foreach (Element elem in elements)
            {
                foreach (int id in elem.Connections)
                {
                    Graphics line1 = panel1.CreateGraphics();
                    line1.DrawLine(pen, elements[elem.id - 1].DOWN, elements[id - 1].UP);
                }
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if(_ids[0] == 0 || _ids[1] == 0 || _ids[0] == _ids[1])
            {
                MessageBox.Show("Выберете пару элементов!", "Пара элементов не была выбрана!");
            }
            else
            {
                elements[_ids[0] - 1].Connections.Add(elements[_ids[1] - 1].id);
                print();
            }       
        }

        private void RouterBtn_Click(object sender, EventArgs e)
        {            
            _id++;
            CreateElement(_id, "Роутер", "ОС", 1, 1, 1, "60, 10", "Router");
        }

        private void ServerBtn_Click(object sender, EventArgs e)
        {

            _id++;
            CreateElement(_id, "Сервер", "ОС", 1, 1, 1, "65, 10", "Server");
        }

        private void FireWallBtn_Click(object sender, EventArgs e)
        {
            _id++;
            CreateElement(_id, "Брандмауэр", "ОС", 1, 1, 1, "70, 10", "Firewall");
        }

        private void PhoneBtn_Click(object sender, EventArgs e)
        {
            _id++;
            CreateElement(_id, "Смартфон", "ОС", 1, 1, 1, "75, 10", "Mobile");
        }     

        
        // вычисления риска сети
        private void Calculation()
        {
            // Общий риск системы
            decimal systemRisk = 0;
            decimal maxElemRisk = 0, maxRisk = 0;
            int id = 0;
            string errors = "";

            foreach (Element element in elements)
            {
                if (element.value == 0 || element.protectionRank == 0)
                {
                    errors += String.Format("Код элемента:{0} - название элемента:{1}; \n" , element.id, element.Name) ;
                }
            }

            if(errors.Length > 0)
            {
                MessageBox.Show(errors, "Параметры защищенности и значимости не могут быть равны нулю!");
            }
            else if(elements.Count < 1)
            {
                MessageBox.Show("Добавьте элементы", "Отсутствуют элементы сети!");
            }
            else
            {
                stopWatch.Start();

                // Для всех элементов системы
                foreach (Element element in elements)
                {
                    // Пройтись по списку угроз
                    for (int i = 0; i < attackInfo.Capacity - 1; i++)
                    {
                        try
                        {
                            if (attackInfo[i][7].Contains(element.OS))  // если Оси совпадают, то 
                            {
                                decimal attackRank = Decimal.Parse(Regex.Match(attackInfo[i][11], @"\d+").Value);
                                element.maxRisk += attackRank  * element.value * element.count;
                                element.risk += (attackRank / element.protectionRank) * element.value * element.count;   // риск элемента += рейтинг атак / рейтинг защищенности
                            }
                        }
                        catch { }
                    }

                    systemRisk += element.risk; // риск системы += риск элемента
                    maxRisk += element.maxRisk;

                    if (element.risk > maxElemRisk)
                    {
                        maxElemRisk = element.risk;
                        id = element.id;
                    }
                }

                stopWatch.Start();
                // по завершению цикла, найти максимальный риск элемента
                ShowResults(systemRisk, maxRisk, id);
            }          
        }

        // отправляем результат вычислений в поле результата
        private void ShowResults(decimal systemRisk, decimal maxRisk, int id)
        {
            string rating = "";
            decimal percent = Math.Round(systemRisk, 0)  / Math.Round(maxRisk, 0);
            percent *= 100;

            if (percent >= 75)
                rating = "критический";
            else if (percent >= 50 && percent < 75)
                rating = "высокий";
            else if (percent >= 25 && percent < 50)
                rating = "средний";
            else
                rating = "низкий";

            id--;
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            resultBox.Text = "Время оценки:" + elapsedTime + Environment.NewLine;
            resultBox.Text += String.Format("Общий риск системы: {0} / {1}; ({2});", Math.Round(systemRisk, 0), Math.Round(maxRisk, 0), rating) + Environment.NewLine;
            resultBox.Text += String.Format("Элемент с максимальным риском: Код - {0}, Название - {1}, Риск - {2}", elements[id].id, elements[id].name, Math.Round(elements[id].risk));
        }

        // читаем файл уязвимостей 
        private void openAttackBtn_Click(object sender, EventArgs e)
        {   
            try
            {
                // Читает файл          
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "CSV Attacks files | *.csv", ValidateNames = true })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        using (var reader = new StreamReader(ofd.FileName))
                        {
                            string line;

                            //Парсим остальные строки
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] cell = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                attackInfo.Add(cell);
                            }
                        }
                        calculateBTN.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
     
        // сохраняем параметры элементов
        private void svaeParams_Click(object sender, EventArgs e)
        {
            elements[_ids[0]-1].name = Convert.ToString(dataGridView[1, 1].Value);
            elements[_ids[0]-1].OS = Convert.ToString(dataGridView[1, 2].Value);
            elements[_ids[0]-1].protectionRank = Convert.ToDecimal(dataGridView[1, 3].Value);
            elements[_ids[0]-1].count = Convert.ToInt32(dataGridView[1, 4].Value);
            elements[_ids[0]-1].value = Convert.ToDecimal(dataGridView[1, 5].Value);
        }

        // сохраняем модель в файл
        private void saveModelBtn_Click(object sender, EventArgs e)
        {         
            saveFileDialog1.Filter = "CSV Files|*.csv";
            saveFileDialog1.Title = "Save an CSV File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog1.FileName != "")
            {
                // Saves via a FileStream created by the OpenFile method.  
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                fs.Close();

                StringBuilder sb = new StringBuilder();
                foreach (Element elem in elements)
                {
                    string nums = "";
                 
                    // добавляем идентификаторы связных элемнтов в список
                    foreach (int num in elem.Connections)
                    {
                        nums += num.ToString() + ',';                      
                    }                                        

                    string elemData = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", elem.id , elem.name , elem.OS , elem.protectionRank , elem.value , elem.count , nums, elem.Location, elem.Type);
                    sb.AppendLine(string.Join(";", elemData));
                }          

                File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
            }           
        }     

        public void CreateElement(int Id, string Name, string os, decimal protRank, decimal value, int count,  string location, string Type, string nums = "")
        {
            string[] coords = location.Split(',');

            Element element = new Element();
            element._print += new Element.print(print);
            
            GetImage(Type);

            element.Image = addImg;
            element.Parent = panel1;
            element.Location = new Point(int.Parse(Regex.Match(coords[0], @"\d+").Value), int.Parse(Regex.Match(coords[1], @"\d+").Value));
            element.UD();
            element.id = Id;
            element.risk = 0;
            element.protectionRank = protRank;
            element.OS = os;
            element.count = count;
            element.name = Name;
            element.value = value;
            element.Type = Type;

            // если есть элементы
            if(nums.Length>0)
            {
                string[] conect = nums.Split(',');

                foreach (string cell in conect)
                {
                    if (cell.Length>0)
                    element.Connections.Add(int.Parse(Regex.Match(cell, @"\d+").Value));
                }
            }
           
            //_id++;

            elements.Add(element);
            element.Click += new System.EventHandler(img_click);
        }

        // присвоить картинку в зависимости от типа
        private void GetImage(string type)
        {
            switch (type)
            {
                case "PC":
                    addImg = Properties.Resources.comp;
                    break;

                case "Router":
                    addImg = Properties.Resources.rout;
                    break;

                case "Switch":
                    addImg = Properties.Resources.commut;
                    break;

                case "Server":
                    addImg = Properties.Resources.serv;
                    break;

                case "Mobile":
                    addImg = Properties.Resources.sphn;
                    break;

                case "Firewall":
                    addImg = Properties.Resources.brend;
                    break;
            }          
        }

        // открыть файл модели
        private void openModelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Читает файл          
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "CSV Model Files| *.csv", ValidateNames = true })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        using (var reader = new StreamReader(ofd.FileName))
                        {
                            string line;

                            //Парсим остальные строки
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] cell = line.Split(new[] { ';' });

                                CreateElement(Convert.ToInt32(cell[0]), cell[1], cell[2], Convert.ToDecimal(cell[3]), Convert.ToDecimal(cell[4]), Convert.ToInt32(cell[5]), cell[7], cell[8], cell[6]);
                            }
                        }
                    }
                }

                Pen pen = new Pen(Color.Black);

                foreach (Element elem in elements)
                {
                    foreach (int id in elem.Connections)
                    {
                        Graphics line1 = panel1.CreateGraphics();
                        line1.DrawLine(pen, elements[elem.id - 1].DOWN, elements[id - 1].UP);
                    }
                }

                _id += elements.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void analyze_Click(object sender, EventArgs e)
        {
            Element highestReskElement = getHighestReskElement();
            List<Element> highestReskPath = getHighestRiskPath(highestReskElement);
            string str = "";
            foreach (Element el in highestReskPath)
            {
                str += Environment.NewLine + el.ToString();
            }

            resultBox.Text += str;
        }

        private void calculateBTN_Click(object sender, EventArgs e)
        {
            Calculation();
            analyze.Enabled = true;
        }

        private Element getHighestReskElement()
        {
            List<Element> sortedElements = elements.OrderBy(element => element.risk).ToList();
            return sortedElements[sortedElements.Count - 1];
        }

        private List<Element> getHighestRiskPath(Element highestRiskElement)
        {
            List<Element> path = new List<Element>();

            path.Add(highestRiskElement);

            for (int i = 0; i < elements.Count; i++ )
            {
                Element element = path[path.Count - 1];
                Element nextElement = getNextInThePathByRisk(element);

                if (element.Connections.Count <1 || path.Contains(nextElement) || nextElement == null)
                {
                    break;
                } 
                else
                {                  
                    path.Add(nextElement);
                }
            }

            return path;
        }

        private List<Element> getConnectedElements(Element element)
        {
            List<int> connections = element.Connections;
            List<Element> connectedElements = new List<Element>();

            foreach (int connection in connections)
            {
                connectedElements.Add(elements[connection-1]);
            }

            return connectedElements;
        }

        private Element getNextInThePathByRisk(Element element)
        {           

            if (element.Connections!=null && element.Connections.Count > 0)
            {
                List<Element> connections = getConnectedElements(element);
                List<Element> sortedConnections = connections.OrderBy(elem => elem.risk).ToList();
                return sortedConnections[sortedConnections.Count - 1];
            }
            else
            {
                return null;
            }
        }
    }
}
