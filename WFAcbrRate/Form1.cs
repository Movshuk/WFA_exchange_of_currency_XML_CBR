﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;

namespace WFAcbrRate
{
   public partial class Form1 : Form
   {
      public Form1()
      {
         InitializeComponent();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         try
         {
            dataGridView1.Rows.Clear(); // очистка строки если вдруг загружен нулевой курс
            //Console.WriteLine("Введите требуемую дату для отображения соотношения стоимости RUB к иностранным валютам ЦБ РФ [ДД.ММ.ГГГГ]");
            //DateTime date = DateTime.ParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

            DateTime date;
            if (DateTime.TryParseExact(textBox1.Text, "dd.MM.yyyy", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out date))
            {
               if (date > DateTime.Now)
               {
                  throw new Exception("Введенная дата больше текущей даты! Курсы не предоставляются на дату больше текущей!");
               }

            }
            else
            {
               throw new Exception("Неверно введена дата!");
            }



            //Console.WriteLine(date.Month + "<<< ");

            //WebRequest request = WebRequest.Create("http://www.cbr.ru/scripts/XML_daily.asp?date_req=29/03/2018");
            WebRequest request = WebRequest.Create("http://www.cbr.ru/scripts/XML_daily.asp?date_req=" + (date.Day < 10 ? ("0" + date.Day.ToString()) : date.Day.ToString()) + "/" + (date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString()) + "/" + date.Year + "\"");

            WebResponse response = request.GetResponse();

            string line = null;

            // чтение результата запроса в строку
            using (Stream stream = response.GetResponseStream())
            {

               StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("windows-1251"));
               line = reader.ReadToEnd();
               reader.Close();
            }

            //Console.WriteLine(line);
            response.Close();
            MessageBox.Show("Запрос выполнен!");
            //Console.WriteLine("Запрос выполнен");
            Console.Read();

            // парс ответа
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(line);

            List<TypeCur> listRates = new List<TypeCur>();

            // чтение блока корневого элемента xml
            // XmlElement xmlRoot = doc.DocumentElement; // корень или короче:
            // string text = doc.DocumentElement.GetAttributeNode("Date").Value.ToString();
            // Console.WriteLine(text + "даты в корне");

            foreach (XmlNode item in doc.DocumentElement)
            {
               /*
               if (item.Attributes.Count > 0)
               {
                  XmlNode attr = item.Attributes.GetNamedItem("ID");
                  if (attr != null)
                     Console.WriteLine(">>>" + attr.Value);
               }
               */
               TypeCur typeCur = new TypeCur();
               typeCur.Date = date;
               foreach (XmlNode childnode in item.ChildNodes)
               {
                  if (childnode.Name == "CharCode")
                  {
                     //Console.WriteLine("КОД: {0}", childnode.InnerText);
                     typeCur.CurAbr = childnode.InnerText;
                  }

                  if (childnode.Name == "Nominal")
                  {
                     //Console.WriteLine("НОМИНАЛ: {0}", childnode.InnerText);
                     typeCur.HowMany = Convert.ToInt32(childnode.InnerText);
                  }

                  if (childnode.Name == "Name")
                  {
                     //Console.WriteLine("ИМЯ ФАЛЮТЫ: {0}", childnode.InnerText);
                     typeCur.CurName = childnode.InnerText;
                  }

                  if (childnode.Name == "Value")
                  {
                     //Console.WriteLine("КУРС: {0}", childnode.InnerText);
                     typeCur.CurRate = Convert.ToDecimal(childnode.InnerText);
                  }
               }
               listRates.Add(typeCur);
            }


            
            foreach (TypeCur tc in listRates)
            {
               dataGridView1.Rows.Add(tc.Date.ToShortDateString(), tc.CurAbr, tc.HowMany, tc.CurName, tc.CurRate);
               //Console.WriteLine(tc.Date.ToShortDateString() + "\t" + tc.CurAbr + "\t" + tc.HowMany + "\t" + tc.CurName + "\t" + tc.CurRate);
            }

            /*
            Console.WriteLine("Press Enter....");
            Console.ReadKey();
            */
         }
         catch (WebException ex)
         {
            if (ex.Response == null) MessageBox.Show("Не возможно установить соединение!");
            //Console.ReadKey();
         }
         catch (Exception ex)
         {
            MessageBox.Show("Ошибка!" + ex.Message);
         }
         /*
         finally
         {
            MessageBox.Show("Программа завершена!");
         }
         */
      }

   }
}
