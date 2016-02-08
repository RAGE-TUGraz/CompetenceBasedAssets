using AssetPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            DomainModelAsset dma = new DomainModelAsset();
            CompetenceAssessmentAsset caa = new CompetenceAssessmentAsset();
            CompetenceRecommendationAsset cra = new CompetenceRecommendationAsset();

            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }
    }
}
