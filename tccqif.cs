using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TCCQIF
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var inputFile = args[0];

                //Tesco download files are in UTF-16 !
                var lines = File.ReadAllLines(inputFile, Encoding.Unicode);

                var converter = new Converter();
                var qif = converter.Convert(lines);

                //Qif should be in ASCII
                var outputFile = Path.ChangeExtension(inputFile, ".qif");
                File.WriteAllText(outputFile, qif, Encoding.ASCII);
            }
            else
            {
                Console.WriteLine("Usage: TCCQIF input.tsv");
            }
        }
    }

    class Converter
    {
        public string Convert(string[] lines)
        {
            var sb = new StringBuilder();

            if (lines.Length > 1)
            {
                //Write a .QIF header
                sb.AppendLine(@"!Type:CCard");

                //Skip the header row
                for (int i = 1; i < lines.Length; i++)
                {
                    if (i > 1)
                        sb.AppendLine("^");

                    var record = lines[i].Split('\t');

                    var transaction = new Transaction();
                    transaction.TransactionDate = record[0].Trim('\"');
                    transaction.PostingDate = record[1].Trim('\"');
                    transaction.BillingAmount = record[2].Trim('\"');
                    transaction.Merchant = record[3].Trim('\"');
                    transaction.MerchantCity = record[4].Trim('\"');
                    transaction.MerchantCounty = record[5].Trim('\"');
                    transaction.MerchantPostalCode = record[6].Trim('\"');
                    transaction.ReferenceNumber = record[7].Trim('\"');
                    transaction.Debit_CreditFlag = record[8].Trim('\"');
                    transaction.SICMCC = record[9].Trim('\"');

                    transaction.Emit(sb);
                }
            }

            return sb.ToString();
        }
    }

    class Transaction
    {
        public string TransactionDate { get; set; }
        public string PostingDate { get; set; }
        public string BillingAmount { get; set; }
        public string Merchant { get; set; }
        public string MerchantCity { get; set; }
        public string MerchantCounty { get; set; }
        public string MerchantPostalCode { get; set; }
        public string ReferenceNumber { get; set; }
        public string Debit_CreditFlag { get; set; }
        public string SICMCC { get; set; }

        public void Emit(StringBuilder sb)
        {
            //Convert the date format
            var date = DateTime.ParseExact(TransactionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            sb.AppendLine("D" + date.ToString("dd/MM/yyyy"));

            //Strip off currency symbols
            var cleanAmount = BillingAmount.TrimStart(new char[] { ' ','£','$'});

            //Change flag to +/-
            if (Debit_CreditFlag == "Credit")
                sb.AppendLine("T" + cleanAmount);
            else if (Debit_CreditFlag == "Debit")
                sb.AppendLine("T-" + cleanAmount);

            sb.AppendLine("P" + Merchant);
            sb.AppendLine("A" + MerchantCity);
            sb.AppendLine("A" + MerchantCounty);
            sb.AppendLine("A" + MerchantPostalCode);
            sb.AppendLine("N" + ReferenceNumber);
        }
    }
}