﻿/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-22
*/

using AssetManagerPackage;
using AssetPackage;
using CompetenceAssessmentAssetNameSpace;
using CompetenceRecommendationAssetNameSpace;
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            /*
            try {
                DomainModelAsset dma2 = new DomainModelAsset();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            */

            //dma.performTests();
            //caa.performTests();
            DomainModel dm = dma.getDomainModel("sdgfgj");
            dm.print();
         

            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }
    }

    class Bridge : IBridge, ILog, IDataStorage, IWebServiceRequest
    {
        #region IDataStorage

        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\" + fileId;
            return(File.Exists(filePath));
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\"+fileId;
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(filePath))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    return (line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error by loading the DM!");
            }

            return (null);
        }

        public void Save(string fileId, string fileData)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\" + fileId;
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.Write(fileData);
            }
        }

        #endregion IDataStorage
        #region ILog

        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("BRIDGE:  " + msg);
        }

        #endregion ILog
        #region IWebServiceRequest

        public void WebServiceRequest(string method, Uri uri, Dictionary<string, string> headers, string body, IWebServiceResponse response)
        {
            String url = body;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                Stream resStream = webResponse.GetResponseStream();

                StreamReader reader = new StreamReader(resStream);
                string dm = reader.ReadToEnd();

                response.Success(url, 2, headers, dm);
            }
            catch(Exception e)
            {
                response.Error(url, e.Message);
            }
        }

        #endregion IWebServiceRequest
    }
}
