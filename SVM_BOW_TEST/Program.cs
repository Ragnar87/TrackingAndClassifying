using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Threading;

namespace SVM_BOW_TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            string positives = "C:\\Users\\Rsg\\Documents\\KT\\EmgucvTest\\EmguTestForm\\trening\\pos";
            string negatives = "C:\\Users\\Rsg\\Documents\\KT\\EmgucvTest\\EmguTestForm\\trening\\neg";
            string test = "C:\\Users\\Rsg\\Documents\\KT\\EmgucvTest\\EmguTestForm\\trening\\test";

            
            List<string> folders = new List<string>();
            folders.Add(positives);
            folders.Add(negatives);
            
            Classifier svm = new Classifier(folders);
            svm.computeAndExtract();
            svm.train();

            FileInfo[] files = new DirectoryInfo(test).GetFiles();

            foreach(FileInfo file in files)
            {
                using (Image<Bgr, Byte> testImg = new Image<Bgr, byte>(file.FullName))
                {
                    string label = file.FullName.Substring(file.FullName.LastIndexOf('\\')+1);
                    float result = svm.classify(testImg);
                    System.Console.WriteLine(label + " " + '\t' + (result == 1 ? "positive" : result == 2 ? "negative" : "undefined"));
                }

            }
            System.Console.Read();
            
            //Classify(laksur, positives, negatives);
        }

    }
}
