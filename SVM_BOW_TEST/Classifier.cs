using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.Structure;
//using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVM_BOW_TEST
{
    public class Classifier
    {
        private List<FileInfo[]> _folders;
        private int class_num = 0;
        private int input_num = 0;
        private Mat tDescriptors;
        private Matrix<int> labels;
        private BOWImgDescriptorExtractor bowDE;
        private Mat vocabulary = new Mat();
        private SURF detector;
        private BFMatcher matcher;
        private SVM svmClassifier;

        public Classifier(List<string> folders)
        {
            _folders = new List<FileInfo[]>();
            svmClassifier = new SVM();

            foreach(string folder in folders)
            {
                try
                {
                    FileInfo[] files = new DirectoryInfo(folder).GetFiles();
                    _folders.Add(files);
                    class_num++;
                }
                catch(DirectoryNotFoundException ex)
                {
                    System.Console.WriteLine(folder + " not found");
                    System.Console.WriteLine(ex.Data);
                    continue;
                }
            }
        }

        public void computeAndExtract()
        {
            using (detector = new SURF(30))
            using (matcher = new BFMatcher(DistanceType.L2))
            {
                bowDE = new BOWImgDescriptorExtractor(detector, matcher);
                BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(100, new MCvTermCriteria(100, 0.01), 3, Emgu.CV.CvEnum.KMeansInitType.PPCenters);

                foreach(FileInfo[] folder in _folders)
                    foreach (FileInfo file in folder)
                    {
                        using (Image<Bgr, Byte> model = new Image<Bgr, byte>(file.FullName))
                        using (VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint())
                        //Detect SURF key points from images
                        {
                            detector.DetectRaw(model, modelKeyPoints);
                            //Compute detected SURF key points & extract modelDescriptors
                            Mat modelDescriptors = new Mat();
                            detector.Compute(model, modelKeyPoints, modelDescriptors);
                            //Add the extracted BoW modelDescriptors into BOW trainer
                            bowTrainer.Add(modelDescriptors);
                        }
                        input_num++;
                    }

                //Cluster the feature vectors
                bowTrainer.Cluster(vocabulary);

                //Store the vocabulary
                bowDE.SetVocabulary(vocabulary);

                //training descriptors
                tDescriptors = new Mat();

                labels = new Matrix<int>(1, input_num);
                int index = 0;
                //compute and store BOWDescriptors and set labels
                for (int i = 1; i <= _folders.Count; i++)
                {
                    FileInfo[] files = _folders[i-1];
                    for (int j = 0; j < files.Length; j++)
                    {
                        FileInfo file = files[j];
                        using (Image<Bgr, Byte> model = new Image<Bgr, Byte>(file.FullName))
                        using (VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint())
                        using (Mat modelBOWDescriptor = new Mat())
                        {
                            detector.DetectRaw(model, modelKeyPoints);
                            bowDE.Compute(model, modelKeyPoints, modelBOWDescriptor);
                            
                            tDescriptors.PushBack(modelBOWDescriptor);
                            labels[0, index++] = i;
                            
                        }
                    }
                }
            }
        }

        public bool train()
        {
            TrainData td = new TrainData(tDescriptors, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, labels);
            bool trained = false;
               
            //set SVM parameters
            svmClassifier.SetKernel(Emgu.CV.ML.SVM.SvmKernelType.Rbf);
            svmClassifier.Gamma = 2;
            svmClassifier.Type = Emgu.CV.ML.SVM.SvmType.CSvc;
            svmClassifier.C = 5;

            trained = svmClassifier.Train(td);
            return trained;            
        }

        public float classify(Image<Bgr, Byte> predImg)
        {
            using (SURF detector = new SURF(30))
            using (BFMatcher matcher = new BFMatcher(DistanceType.L2))
            using (Image<Gray, Byte> testImgGray = predImg.Convert<Gray, Byte>())
            using (VectorOfKeyPoint testKeyPoints = new VectorOfKeyPoint())
            using (Mat testBOWDescriptor = new Mat())
            using( bowDE = new BOWImgDescriptorExtractor(detector, matcher))
            {
                float result = 0;
                bowDE.SetVocabulary(vocabulary);
                detector.DetectRaw(predImg, testKeyPoints, null);
                bowDE.Compute(predImg, testKeyPoints, testBOWDescriptor);
                if(!testBOWDescriptor.IsEmpty)
                    result = svmClassifier.Predict(testBOWDescriptor);

                //result will indicate whether test image belongs to trainDescriptor label 1, 2
                return result;
            }
        }
    }
}
