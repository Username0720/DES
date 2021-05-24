using System;
using System.Windows.Forms;

namespace DES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string[] Blocks; //блоки в двоичном формате
        //в DES размер блока 64 бит, но поскольку в unicode символ в два раза длинее, то увеличим блок тоже в два раза
        int sizeOfBlock = 128;
        int shiftKey = 2; //сдвиг ключа
        int sizeOfChar = 16; //размер одного символа (in Unicode 16 bit)
        //Метод, доводящий строку до такого размера, чтобы она делилась на sizeOfBlock. 
        //Иначе размер увеличиваем с помощью добавления к исходной строке символа “0”.
        private string StringToRightLength(string input)
        {
            while (((input.Length * sizeOfChar) % sizeOfBlock) != 0)
                input += "0";

            return input;
        }
        //ксорим строки в двоичном виде
        private string XOR(string s1, string s2)
        {
            string result = "";
            for (int i = 0; i < s1.Length; i++)
            {
                bool a = Convert.ToBoolean(Convert.ToInt32(s1[i].ToString()));
                bool b = Convert.ToBoolean(Convert.ToInt32(s2[i].ToString()));

                if (a ^ b)
                    result += "1";
                else
                    result += "0";
            }
            return result;
        }
        
        //длину ключа доводим до нужной
        private string KeyWordCorrect(string input, int Key_length)
        {
            if (input.Length > Key_length)
                input = input.Substring(0, Key_length);
            else
                while (input.Length < Key_length)
                    input = "0" + input;
            return input;
        }
        //разбиваем строку на блоки
        private void StringIntoBlocks(string input)
        {
            Blocks = new string[(input.Length * sizeOfChar) / sizeOfBlock];
            int lengthOfBlock = input.Length / Blocks.Length;
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i] = input.Substring(i * lengthOfBlock, lengthOfBlock);
                Blocks[i] = Format_StringToBinary(Blocks[i]);
            }
        }
        //разбиваем двоичную строку на блоки
        private void BinaryStringIntoBlocks(string input)
        {
            Blocks = new string[input.Length / sizeOfBlock];
            int lengthOfBlock = input.Length / Blocks.Length;
            for (int i = 0; i < Blocks.Length; i++)
                Blocks[i] = input.Substring(i * lengthOfBlock, lengthOfBlock);
        }
        //символьный переводим в двоичный формат
        private string Format_StringToBinary(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                string char_binary = Convert.ToString(input[i], 2);
                while (char_binary.Length < sizeOfChar)
                    char_binary = "0" + char_binary;
                output += char_binary;
            }
            return output;
        }
        //двоичные переводим в символьный формат
        private string Format_BinaryToString(string input)
        {
            string output = "";
            while (input.Length > 0)
            {
                string char_binary = input.Substring(0, sizeOfChar);
                input = input.Remove(0, sizeOfChar);
                int a = 0;
                int degree = char_binary.Length - 1;
                foreach (char c in char_binary)
                    a += Convert.ToInt32(c.ToString()) * (int)Math.Pow(2, degree--);
                output += ((char)a).ToString();
            }
            return output;
        }
        //один раунд шифрования
        private string Encode_Round(string input, string key)
        {
            string L = input.Substring(0, input.Length / 2);
            string R = input.Substring(input.Length / 2, input.Length / 2);
            return (R + XOR(L, XOR(R, key)));
        }
        //один раунд расшифрования
        private string Decode_Round(string input, string key)
        {
            string L = input.Substring(0, input.Length / 2);
            string R = input.Substring(input.Length / 2, input.Length / 2);
            return (XOR(XOR(L, key), R) + L);
        }
        //циклический сдвиг shiftKey
        private string NextRoundKey(string key)
        {
            for (int i = 0; i < shiftKey; i++)
            {
                key = key[key.Length - 1] + key;
                key = key.Remove(key.Length - 1);
            }
            return key;
        }
        //циклический сдвиг shiftKey для расшифровки
        private string PrevRoundKey(string key)
        {
            for (int i = 0; i < shiftKey; i++)
            {
                key = key + key[0];
                key = key.Remove(0, 1);
            }
            return key;
        }
        //cipher
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && textBox3.Text.Length > 0)
            {
                string key = textBox1.Text;
                string sr = textBox3.Text;

                sr = StringToRightLength(sr);

                StringIntoBlocks(sr);

                key = KeyWordCorrect(key, sr.Length / (2 * Blocks.Length));
                textBox1.Text = key;
                key = Format_StringToBinary(key);

                for (int j = 0; j < 16; j++)
                {
                    for (int i = 0; i < Blocks.Length; i++)
                        Blocks[i] = Encode_Round(Blocks[i], key);

                    key = NextRoundKey(key);
                }

                key = PrevRoundKey(key);

                textBox2.Text = Format_BinaryToString(key);

                string result = "";

                for (int i = 0; i < Blocks.Length; i++)
                    result += Blocks[i];

                textBox4.Text = Format_BinaryToString(result);
            }
            else
                MessageBox.Show("Заполните все поля для зашифровки!");
        }
        //decipher
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0 && textBox4.Text.Length > 0)
            {
                string key = Format_StringToBinary(textBox2.Text);
                string sr = textBox4.Text;

                sr = Format_StringToBinary(sr);

                BinaryStringIntoBlocks(sr);

                for (int j = 0; j < 16; j++)
                {
                    for (int i = 0; i < Blocks.Length; i++)
                        Blocks[i] = Decode_Round(Blocks[i], key);

                    key = PrevRoundKey(key);
                }

                key = NextRoundKey(key);

                textBox1.Text = Format_BinaryToString(key);

                string result = "";

                for (int i = 0; i < Blocks.Length; i++)
                    result += Blocks[i];

                textBox3.Text = Format_BinaryToString(result);
            }
            else
                MessageBox.Show("Заполните все поля для расшифровки!");
        }
    }
}
