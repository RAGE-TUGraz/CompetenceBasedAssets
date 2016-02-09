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
            AssetManager am = AssetManager.Instance;
            am.Bridge = new Bridge();

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
            

            Console.WriteLine("Searching domainModelAsset....");
            Console.WriteLine(am.findAssetByClass("DomainModelAsset").Id);
            

            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }
    }

    class Bridge : IBridge, ILog
    {
        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("BRIDGE:  "+msg);
        }
    }
}
