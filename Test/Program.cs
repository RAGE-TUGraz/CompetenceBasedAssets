using AssetManagerPackage;
using AssetPackage;
using CompetenceAssessmentAssetNameSpace;
using CompetenceRecommendationAssetNameSpace;
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCompetence
{
    class Program
    {
        static void Main(string[] args)
        {
            DomainModelAsset dma = new DomainModelAsset();
            CompetenceAssessmentAsset caa = new CompetenceAssessmentAsset();
            CompetenceRecommendationAsset cra = new CompetenceRecommendationAsset();

            try {
                DomainModelAsset dma2 = new DomainModelAsset();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine(dma.Id);

            AssetManager am = AssetManager.Instance;

            Console.WriteLine("Searching domainModelAsset....");
            Console.WriteLine(am.findAssetByClass("DomainModelAsset").Id);
            

            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }
    }
}
