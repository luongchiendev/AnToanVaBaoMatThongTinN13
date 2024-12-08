using Microsoft.Win32;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace DSA
{
    public partial class MainWindow : Window
    {
        public BigInteger p, q, g, x, y;

        public MainWindow()
        {
            InitializeComponent();
            setvalue(textq, textp);
        }

        public BigInteger getvalue(TextBox text)
        {
            BigInteger x = BigInteger.Parse(text.Text);
            return x;
        }

        public void setvalue(TextBox textq, TextBox textp)
        {
            q = GenerateRandomPrime(10); // Chọn số nguyên tố q với 10 bit

            do
            {
                BigInteger k = GenerateRandomNumber(10); // Chọn số nguyên k với 10 bit
                p = k * q + 1; // Tính p = kq + 1
            } while (!IsPrime(p)); // Kiểm tra xem p có phải là số nguyên tố không

            textq.Text = q.ToString();
            textp.Text = p.ToString();
        }

        private void createkey_Click(object sender, RoutedEventArgs e)
        {
            p = getvalue(textp);
            q = getvalue(textq);
            BigInteger h;
            do
            {
                h = GenerateRandomNumber((int)BigInteger.Log(p, 2) - 1); // Chọn số nguyên h
            } while (h <= 1 || h >= p || ModPow(h, (p - 1) / q, p) == 1); // Đảm bảo 1 < h < p-1 và h^(p-1)/q != 1 mod p
            g = ModPow(h, (p - 1) / q, p);
            // Bước 2: Sinh cặp khóa
            x = GenerateRandomNumberLessThan(q); // Chọn số ngẫu nhiên x (0 < x < q)
            y = ModPow(g, x, p); // Tính y = g^x mod p
            if (!IsPrime(p))
            {
                MessageBox.Show("p phai la so nguyen to", "thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if ((p - 1) % q != 0)
            {
                MessageBox.Show("q phai la uoc cua p-1", "thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            setkey(valueq, valuep, valueg, publickey, privatekey);
        }

        public void setkey(TextBox valueq, TextBox valuep, TextBox valueg, TextBox publickey, TextBox privatekey)
        {
            valuep.Text = p.ToString();
            valueq.Text = q.ToString();
            valueg.Text = g.ToString();
            publickey.Text = y.ToString();
            privatekey.Text = x.ToString();
        }

        public BigInteger m, r, s, v;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            setvalue(textq, textp);
            valuep.Text = "";
            valueq.Text = "";
            valueg.Text = "";
            publickey.Text = "";
            privatekey.Text = "";
            masage.Text = "";
            textresult.Text = "";
            checkmasage.Text = "";
            checkresult.Text = "";
            notyfy.Text = "";
        }

        string message = "";
        string str = "";
        private void create_Click(object sender, RoutedEventArgs e)
        {
            if (masage.Text == "")
            {
                MessageBox.Show("chưa có thông điệp", "thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            message = masage.Text;
            x = getvalue(privatekey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            m = new BigInteger(SHA256.Create().ComputeHash(messageBytes)) % q; // Chuyển thông điệp thành số nguyên
            // Bước 3: Ký dữ liệu
            BigInteger kSign;
            do
            {
                kSign = GenerateRandomNumberLessThan(q); // Chọn số ngẫu nhiên k (0 < k < q)
            } while (kSign <= 0 || kSign >= q);

            r = ModPow(g, kSign, p) % q; // Tính r = (g^k mod p) mod q
            BigInteger kInverse = ModInverse(kSign, q); // Tính k^(-1) mod q
            s = (kInverse * (m + x * r)) % q; // Tính s = (k^(-1) * (H(m) + x * r)) mod q

            str = $"{r} , {s}";
            textresult.Text = str;

            checkmasage.Text = message;
            checkresult.Text = str;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!message.Equals(checkmasage.Text) && !str.Equals(checkresult.Text))
            {
                notyfy.Text = "false";
                return;
            }

            if (!message.Equals(checkmasage.Text))
            {
                notyfy.Text = "false"; return;
            }
            if (!str.Equals(checkresult.Text))
            {
                notyfy.Text = "false"; return;
            }
            // Bước 4: Xác minh chữ ký
            BigInteger w = ModInverse(s, q); // Tính w = s^(-1) mod q
            BigInteger u1 = (m * w) % q; // Tính u1 = (H(m) * w) mod q
            BigInteger u2 = (r * w) % q; // Tính u2 = (r * w) mod q
            BigInteger v = (ModPow(g, u1, p) * ModPow(y, u2, p) % p) % q; // Tính v = (g^u1 * y^u2 mod p) mod q

            // Kiểm tra chữ ký
            bool isValid = (v == r);
            notyfy.Text = isValid.ToString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file";
            openFileDialog.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Image files (*.png;*.jpg)|*.png;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    masage.Text = fileContent;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while reading the file: " + ex.Message);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string filePath = @"C:\Users\bachb\OneDrive\Desktop\Chuki\filechuky.txt";

            // Dữ liệu từ TextBox
            string data = textresult.Text;
            try
            {
                // Tạo đối tượng StreamWriter để ghi dữ liệu vào file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(data);
                }

                MessageBox.Show("Dữ liệu đã được lưu thành công vào file .txt", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void textresult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public BigInteger GenerateRandomPrime(int bitLength)
        {
            BigInteger p;
            do
            {
                p = GenerateRandomNumber(bitLength);
            } while (!IsPrime(p));
            return p;
        }

        // Hàm sinh số nguyên ngẫu nhiên với độ dài bit
        public BigInteger GenerateRandomNumber(int bitLength)
        {
            if (bitLength < 1)
                throw new ArgumentOutOfRangeException(nameof(bitLength), "bitLength phải lớn hơn 0");

            int byteLength = (bitLength + 7) / 8;
            byte[] bytes = new byte[byteLength];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Đảm bảo số không vượt quá độ dài bit
            if (bitLength % 8 != 0)
            {
                byte mask = (byte)(0xFF >> (8 - (bitLength % 8)));
                bytes[bytes.Length - 1] &= mask;
            }

            // Set bit cao nhất để đảm bảo độ dài bit chính xác
            if (bitLength > 0)
                bytes[bytes.Length - 1] |= (byte)(1 << ((bitLength - 1) % 8));

            BigInteger number = new BigInteger(bytes);

            return number;
        }

        // Hàm kiểm tra số nguyên tố
        public bool IsPrime(BigInteger number)
        {
            if (number < 2) return false;
            if (number % 2 == 0) return number == 2;
            for (BigInteger i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        // Hàm tính lũy thừa modulo
        public BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
        {
            return BigInteger.ModPow(value, exponent, modulus);
        }

        // Hàm tính nghịch đảo modulo
        public BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;
            if (m == 1)
                return 0;
            while (a > 1)
            {
                q = a / m;
                t = m;
                m = a % m;
                a = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }
            if (x1 < 0)
                x1 += m0;
            return x1;
        }

        // Hàm tính độ dài bit của số nguyên BigInteger
        public int GetBitLength(BigInteger number)
        {
            if (number == 0) return 1; // Nếu number là 0 thì trả về 1 (độ dài bit 1)

            int bitLength = 0;
            while (number > 0)
            {
                number >>= 1; // Dịch phải để kiểm tra từng bit
                bitLength++;
            }
            return bitLength;
        }

        // Hàm sinh số nguyên ngẫu nhiên trong khoảng từ 0 đến q
        public BigInteger GenerateRandomNumberLessThan(BigInteger q)
        {
            BigInteger number;
            do
            {
                number = GenerateRandomNumber(GetBitLength(q)); // Sử dụng độ dài bit của q để sinh số ngẫu nhiên
            } while (number >= q);

            return number;
        }

    }
}
