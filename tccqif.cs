using Microsoft.VisualBasic.FileIO;
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
                using (var input = new StreamReader(inputFile, Encoding.Unicode))
                {
                    var converter = new Converter();
                    var qif = converter.Convert(input, Path.GetExtension(inputFile));

                    //Qif should be in ASCII
                    var outputFile = Path.ChangeExtension(inputFile, ".qif");
                    File.WriteAllText(outputFile, qif, Encoding.ASCII);
                }
            }
            else
            {
                Console.WriteLine("Usage: TCCQIF input.tsv");
            }
        }
    }

    class Converter
    {
        public string Convert(TextReader input, string extension)
        {
            var sb = new StringBuilder();

            using (var parser = new TextFieldParser(input))
            {
                if (extension.Equals(".tsv", StringComparison.InvariantCultureIgnoreCase))
                    parser.SetDelimiters("\t");
                else if (extension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
                    parser.SetDelimiters(",");
                else
                    throw new Exception("Unrecognised file extension : " + extension);

                parser.TextFieldType = FieldType.Delimited;
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = true;

                //QIF header
                sb.AppendLine(@"!Type:CCard");

                //Skip header
                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    var record = parser.ReadFields();

                    if (record.Length != 10)
                        throw new Exception("Record " + parser.LineNumber + " doesn't have 10 fields");

                    var transaction = new Transaction();
                    transaction.TransactionDate = record[0];
                    transaction.PostingDate = record[1];
                    transaction.BillingAmount = record[2];
                    transaction.Merchant = record[3];
                    transaction.MerchantCity = record[4];
                    transaction.MerchantCounty = record[5];
                    transaction.MerchantPostalCode = record[6];
                    transaction.ReferenceNumber = record[7];
                    transaction.Debit_CreditFlag = record[8];
                    transaction.SICMCC = record[9];
                    transaction.Emit(sb);
                    sb.AppendLine("^");
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
            var cleanAmount = BillingAmount.TrimStart(new char[] { ' ', '£', '$' });

            //Change flag to +/-
            if (Debit_CreditFlag.Equals("Credit", StringComparison.InvariantCultureIgnoreCase))
                sb.AppendLine("T" + cleanAmount);
            else if (Debit_CreditFlag.Equals("Debit", StringComparison.InvariantCultureIgnoreCase))
                sb.AppendLine("T-" + cleanAmount);
            else
                throw new Exception("Unrecognised debit/credit flag : " + Debit_CreditFlag);

            sb.AppendLine("P" + Merchant);
            sb.AppendLine("A" + MerchantCity);
            sb.AppendLine("A" + MerchantCounty);
            sb.AppendLine("A" + MerchantPostalCode);
            sb.AppendLine("N" + ReferenceNumber);
        }
    }
}
