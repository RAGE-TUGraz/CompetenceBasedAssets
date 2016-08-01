/*
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
*/

using AssetManagerPackage;
using AssetPackage;
using CompetenceAssessmentAssetNameSpace;
using CompetenceBasedAdaptionAssetNameSpace;
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace TestCompetence
{
    class Program
    {
        static void Main(string[] args)
        {
            //create Asset Manager and assign Bridge
            AssetManager am = AssetManager.Instance;
            am.Bridge = new Bridge();

            //create Assets to be tested
            DomainModelAsset dma = new DomainModelAsset();
            CompetenceAssessmentAsset caa = new CompetenceAssessmentAsset();
            CompetenceBasedAdaptionAsset cra = new CompetenceBasedAdaptionAsset();

            //test the Domain Model Assets
            TestDomainModelAsset tdm = new TestDomainModelAsset();
            tdm.performAllTests();

            //test the Competence Assessment Asset
            TestCompetenceAssessmentAsset tca = new TestCompetenceAssessmentAsset();
            tca.performAllTests();

            //test the Competence based Adaptation Asset
            TestCompetenceBasedAdaptationAsset taba = new TestCompetenceBasedAdaptationAsset();
            taba.performAllTests();
            
            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }
    }

    class TestDomainModelAsset
    {
        #region HelperMethods
        /// <summary>
        /// Logging functionality for the Domain Model Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            ILog logger = (ILog) AssetManager.Instance.Bridge;
            logger.Log(severity, "[DMA Test]"  + msg);
        }

        /// <summary>
        /// Method returning the Domain Model Asset
        /// </summary>
        /// <returns> The Doamin Model Asset</returns>
        public DomainModelAsset getDMA()
        {
            return (DomainModelAsset) AssetManager.Instance.findAssetByClass("DomainModelAsset");
        }
        
        /// <summary>
        /// Method creating an example domain model
        /// </summary>
        /// <returns></returns>
        public DomainModel createExampleDomainModel()
        {

            DomainModel dm = new DomainModel();


            //Competences
            Elements elements = new Elements();
            CompetenceList cl = new CompetenceList();
            CompetenceDesc cd1 = new CompetenceDesc("C1");
            CompetenceDesc cd2 = new CompetenceDesc("C2");
            CompetenceDesc cd3 = new CompetenceDesc("C3");
            CompetenceDesc cd4 = new CompetenceDesc("C4");
            CompetenceDesc cd5 = new CompetenceDesc("C5");
            CompetenceDesc cd6 = new CompetenceDesc("C6");
            CompetenceDesc cd7 = new CompetenceDesc("C7");
            CompetenceDesc cd8 = new CompetenceDesc("C8");
            CompetenceDesc cd9 = new CompetenceDesc("C9");
            CompetenceDesc cd10 = new CompetenceDesc("C10");
            CompetenceDesc[] cdArray = { cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10 };
            List<CompetenceDesc> cdList = new List<CompetenceDesc>(cdArray);
            cl.competenceList = cdList;
            elements.competences = cl;

            //Game situations
            SituationsList sl = new SituationsList();
            Situation s1 = new Situation("gs1");
            Situation s2 = new Situation("gs2");
            Situation s3 = new Situation("gs3");
            Situation s4 = new Situation("gs4");
            Situation s5 = new Situation("gs5");
            Situation s6 = new Situation("gs6");
            Situation s7 = new Situation("gs7");
            Situation s8 = new Situation("gs8");
            Situation s9 = new Situation("gs9");
            Situation s10 = new Situation("gs10");
            Situation[] sArray = { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 };
            List<Situation> loList = new List<Situation>(sArray);
            sl.situationList = loList;
            elements.situations = sl;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", new String[] { "C1", "C2" });
            CompetenceP cp3 = new CompetenceP("C6", new String[] { "C4" });
            CompetenceP cp4 = new CompetenceP("C7", new String[] { "C4" });
            CompetenceP cp5 = new CompetenceP("C8", new String[] { "C3", "C6" });
            CompetenceP cp7 = new CompetenceP("C9", new String[] { "C5", "C8" });
            CompetenceP cp8 = new CompetenceP("C10", new String[] { "C9", "C7" });
            CompetenceP[] cpArray = { cp1, cp3, cp4, cp5, cp7, cp8 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            SituationRelationList lorl = new SituationRelationList();
            SituationRelation lor1 = new SituationRelation("gs1", new String[] { "C1" });
            SituationRelation lor2 = new SituationRelation("gs2", new String[] { "C2" });
            SituationRelation lor3 = new SituationRelation("gs3", new String[] { "C3" });
            SituationRelation lor4 = new SituationRelation("gs4", new String[] { "C4" });
            SituationRelation lor5 = new SituationRelation("gs5", new String[] { "C5", "C1", "C2" });
            SituationRelation lor8 = new SituationRelation("gs6", new String[] { "C6", "C4" });
            SituationRelation lor10 = new SituationRelation("gs7", new String[] { "C4", "C7" });
            SituationRelation lor12 = new SituationRelation("gs8", new String[] { "C8", "C6", "C3" });
            SituationRelation lor15 = new SituationRelation("gs9", new String[] { "C9", "C5", "C8" });
            SituationRelation lor18 = new SituationRelation("gs10", new String[] { "C10", "C9", "C7" });
            SituationRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5, lor8, lor10, lor12, lor15, lor18 };
            List<SituationRelation> lorList = new List<SituationRelation>(lorArray);
            lorl.situations = lorList;
            relations.situations = lorl;

            dm.elements = elements;
            dm.relations = relations;

            //Update Levels
            UpdateLevel ul1 = new UpdateLevel();
            ul1.direction = "up";
            ul1.power = "low";
            ul1.xi = "1.2";
            ul1.minonecompetence = "false";
            ul1.maxonelevel = "true";
            UpdateLevel ul2 = new UpdateLevel();
            ul2.direction = "up";
            ul2.power = "medium";
            ul2.xi = "2";
            ul2.minonecompetence = "false";
            ul2.maxonelevel = "true";
            UpdateLevel ul3 = new UpdateLevel();
            ul3.direction = "up";
            ul3.power = "high";
            ul3.xi = "4";
            ul3.minonecompetence = "true";
            ul3.maxonelevel = "false";
            UpdateLevel ul4 = new UpdateLevel();
            ul4.direction = "down";
            ul4.power = "low";
            ul4.xi = "1.2";
            ul4.minonecompetence = "false";
            ul4.maxonelevel = "true";
            UpdateLevel ul5 = new UpdateLevel();
            ul5.direction = "down";
            ul5.power = "medium";
            ul5.xi = "2";
            ul5.minonecompetence = "false";
            ul5.maxonelevel = "true";
            UpdateLevel ul6 = new UpdateLevel();
            ul6.direction = "down";
            ul6.power = "high";
            ul6.xi = "4";
            ul6.minonecompetence = "true";
            ul6.maxonelevel = "false";
            UpdateLevel[] ulArray = { ul1, ul2, ul3, ul4, ul5, ul6 };
            dm.updateLevels = new UpdateLevels();
            dm.updateLevels.updateLevelList = new List<UpdateLevel>(ulArray);

            return dm;
        }

        #endregion HelperMethods
        #region TestMethods

        /// <summary>
        /// Method starting all Tests
        /// </summary>
        public void performAllTests()
        {
            log("Domain model asset tests called: ");
            performTest1();
            performTest2();
            performTest3();
            log("Domain model asset tests finished. ");
        }

        /// <summary>
        /// Creates example DomainModel; stores, loads and outputs this DomainModel from File.
        /// </summary>
        private void performTest1()
        {
            DomainModel dm = createExampleDomainModel();
            string fileId = "DomainModelTestId.xml";
            
            //store dm to file
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            //change Settings to load local file
            DomainModelAssetSettings newDMAS = new DomainModelAssetSettings();
            newDMAS.LocalSource = true;
            newDMAS.Source = fileId;
            getDMA().Settings = newDMAS;

            //load domain model
            DomainModel dm2 = getDMA().getDomainModel();

            //check domain model
            if (!dm.toXmlString().Equals(dm2.toXmlString()))
            {
                log("DomainModelAsset - Test1 failed!", Severity.Error);
                throw new Exception("EXCEPTION: DomainModelAsset - Test1 failed!");
            }
            else
            {
                log("DomainModelAsset - Test1 passed.");
            }
            
        }

        /// <summary>
        /// Loads web-domain model.
        /// </summary>
        private void performTest2()
        {
            log("DomainModelAsset: Starting Test 2");
            DomainModelAssetSettings dmas = new DomainModelAssetSettings();
            dmas.WebSource = true;
            dmas.Source = @"http://css-kmi.tugraz.at:8080/compod/rest/getdomainmodel?id=isr2013";
            getDMA().Settings = dmas;

            try
            {
                DomainModel dm = getDMA().getDomainModel();
                log(dm.toXmlString());
            }
            catch (Exception e)
            {
                log(e.Message);
                log("Maybe the uri (" + dmas.Source + ") is not valid any more.");
            }
            
            log("DomainModelAsset: Ending Test 2");
        }
        
        /// <summary>
        /// Loading and printing local domain model
        /// </summary>
        private void performTest3()
        {
            log("DomainModelAsset: Start Test 3");

            DomainModel dm = createExampleDomainModel();
            string fileId = "DomainModelTestId2.xml";

            //store dm to file
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            DomainModelAssetSettings dmas = new DomainModelAssetSettings();
            dmas.WebSource = false;
            dmas.Source = "DomainModelTestId2.xml";
            getDMA().Settings = dmas;

            try
            {
                getDMA().getDomainModel().print();
            }
            catch (Exception e)
            {
                log("Loading of domain model 'DomainModelTestId2.xml' not possible.");
                log(e.Message);
            }
            
            log("DomainModelAsset: End Test 3");
        }
        
        #endregion TestMethods
    }

    class TestCompetenceAssessmentAsset
    {
        #region HelperMethods

        /// <summary>
        /// Logging functionality for the Competence Assessment Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            ILog logger = (ILog)AssetManager.Instance.Bridge;
            logger.Log(severity, "[CAA Test]" + msg);
        }

        /// <summary>
        /// Method returning the Competence Assessment Asset
        /// </summary>
        /// <returns> The Doamin Model Asset</returns>
        public CompetenceAssessmentAsset getCAA()
        {
            return (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
        }

        /// <summary>
        /// Method creating an example domain model
        /// </summary>
        /// <returns></returns>
        public DomainModel createExampleDomainModel()
        {

            DomainModel dm = new DomainModel();


            //Competences
            Elements elements = new Elements();
            CompetenceList cl = new CompetenceList();
            CompetenceDesc cd1 = new CompetenceDesc("C1");
            CompetenceDesc cd2 = new CompetenceDesc("C2");
            CompetenceDesc cd3 = new CompetenceDesc("C3");
            CompetenceDesc cd4 = new CompetenceDesc("C4");
            CompetenceDesc cd5 = new CompetenceDesc("C5");
            CompetenceDesc cd6 = new CompetenceDesc("C6");
            CompetenceDesc cd7 = new CompetenceDesc("C7");
            CompetenceDesc cd8 = new CompetenceDesc("C8");
            CompetenceDesc cd9 = new CompetenceDesc("C9");
            CompetenceDesc cd10 = new CompetenceDesc("C10");
            CompetenceDesc[] cdArray = { cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10 };
            List<CompetenceDesc> cdList = new List<CompetenceDesc>(cdArray);
            cl.competenceList = cdList;
            elements.competences = cl;

            //Game situations
            SituationsList sl = new SituationsList();
            Situation s1 = new Situation("gs1");
            Situation s2 = new Situation("gs2");
            Situation s3 = new Situation("gs3");
            Situation s4 = new Situation("gs4");
            Situation s5 = new Situation("gs5");
            Situation s6 = new Situation("gs6");
            Situation s7 = new Situation("gs7");
            Situation s8 = new Situation("gs8");
            Situation s9 = new Situation("gs9");
            Situation s10 = new Situation("gs10");
            Situation[] sArray = { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 };
            List<Situation> loList = new List<Situation>(sArray);
            sl.situationList = loList;
            elements.situations = sl;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", new String[] { "C1", "C2" });
            CompetenceP cp3 = new CompetenceP("C6", new String[] { "C4" });
            CompetenceP cp4 = new CompetenceP("C7", new String[] { "C4" });
            CompetenceP cp5 = new CompetenceP("C8", new String[] { "C3", "C6" });
            CompetenceP cp7 = new CompetenceP("C9", new String[] { "C5", "C8" });
            CompetenceP cp8 = new CompetenceP("C10", new String[] { "C9", "C7" });
            CompetenceP[] cpArray = { cp1, cp3, cp4, cp5, cp7, cp8 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            SituationRelationList lorl = new SituationRelationList();
            SituationRelation lor1 = new SituationRelation("gs1", new String[] { "C1" });
            SituationRelation lor2 = new SituationRelation("gs2", new String[] { "C2" });
            SituationRelation lor3 = new SituationRelation("gs3", new String[] { "C3" });
            SituationRelation lor4 = new SituationRelation("gs4", new String[] { "C4" });
            SituationRelation lor5 = new SituationRelation("gs5", new String[] { "C5", "C1", "C2" });
            SituationRelation lor8 = new SituationRelation("gs6", new String[] { "C6", "C4" });
            SituationRelation lor10 = new SituationRelation("gs7", new String[] { "C4", "C7" });
            SituationRelation lor12 = new SituationRelation("gs8", new String[] { "C8", "C6", "C3" });
            SituationRelation lor15 = new SituationRelation("gs9", new String[] { "C9", "C5", "C8" });
            SituationRelation lor18 = new SituationRelation("gs10", new String[] { "C10", "C9", "C7" });
            SituationRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5, lor8, lor10, lor12, lor15, lor18 };
            List<SituationRelation> lorList = new List<SituationRelation>(lorArray);
            lorl.situations = lorList;
            relations.situations = lorl;

            dm.elements = elements;
            dm.relations = relations;

            //Update Levels
            UpdateLevel ul1 = new UpdateLevel();
            ul1.direction = "up";
            ul1.power = "low";
            ul1.xi = "1.2";
            ul1.minonecompetence = "false";
            ul1.maxonelevel = "true";
            UpdateLevel ul2 = new UpdateLevel();
            ul2.direction = "up";
            ul2.power = "medium";
            ul2.xi = "2";
            ul2.minonecompetence = "false";
            ul2.maxonelevel = "true";
            UpdateLevel ul3 = new UpdateLevel();
            ul3.direction = "up";
            ul3.power = "high";
            ul3.xi = "4";
            ul3.minonecompetence = "true";
            ul3.maxonelevel = "false";
            UpdateLevel ul4 = new UpdateLevel();
            ul4.direction = "down";
            ul4.power = "low";
            ul4.xi = "1.2";
            ul4.minonecompetence = "false";
            ul4.maxonelevel = "true";
            UpdateLevel ul5 = new UpdateLevel();
            ul5.direction = "down";
            ul5.power = "medium";
            ul5.xi = "2";
            ul5.minonecompetence = "false";
            ul5.maxonelevel = "true";
            UpdateLevel ul6 = new UpdateLevel();
            ul6.direction = "down";
            ul6.power = "high";
            ul6.xi = "4";
            ul6.minonecompetence = "true";
            ul6.maxonelevel = "false";
            UpdateLevel[] ulArray = { ul1, ul2, ul3, ul4, ul5, ul6 };
            dm.updateLevels = new UpdateLevels();
            dm.updateLevels.updateLevelList = new List<UpdateLevel>(ulArray);

            return dm;
        }

        /// <summary>
        /// Method for setting the loaded domain model
        /// </summary>
        /// <param name="dm"> The domain model to load</param>
        public void setDomainModel(DomainModel dm)
        {
            string fileId = "CompetenceAssessmentAssetTestId.xml";

            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            //change Settings to load local file
            DomainModelAssetSettings newDMAS = new DomainModelAssetSettings();
            newDMAS.LocalSource = true;
            newDMAS.Source = fileId;
            ((DomainModelAsset) AssetManager.Instance.findAssetByClass("DomainModelAsset")).Settings = newDMAS;
        }

        /// <summary>
        /// Method for printing the current competence state
        /// </summary>
        public void printCS()
        {
            log("Competence State:");
            CompetenceAssessmentAsset caa = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
            Dictionary<string,double> cs = caa.getCompetenceState();
            String str = "";
            foreach (var pair in cs)
            {
                str += "(" + pair.Key + ":" + Math.Round(pair.Value, 2) + ")";
            }
            log(str);
            
        }

        #endregion HelperMethods
        #region TestMethods
        
        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            log("Competence assessment asset tests called: ");

            performTest1();
            performTest2();
            performTest3();
            performTest4();
            performTest5();
            /*
            performTest6();
            performTest7();
            performTest8();
            */
            log("Competence assessment asset tests finished. ");
        }
        
        /// <summary>
        /// Creates example domainmodel through the domainmodelhandler and performes some updates.
        /// </summary>
        private void performTest1()
        {
            log("Start Test 1");

            DomainModel dm = createExampleDomainModel();
            setDomainModel(dm);
            printCS();

            //first update - upgrade
            List<String> compList = new List<string>();
            List<Boolean> evidenceList = new List<Boolean>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            compList.Add("C1");
            evidenceList.Add(true);
            evidencePowers.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState(compList, evidenceList, evidencePowers);
            printCS();

            //second update - downgrade
            List<String> compList2 = new List<string>();
            List<Boolean> evidenceList2 = new List<Boolean>();
            List<EvidencePower> evidencePowers2 = new List<EvidencePower>();
            compList2.Add("C1");
            evidenceList2.Add(false);
            evidencePowers2.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState(compList2, evidenceList2, evidencePowers2);
            printCS();
            log("End Test 1");
        }


        /// <summary>
        /// Test method printing out an example domain model. 
        /// </summary>
        private void performTest2()
        {
            log("Start Test 2");
            DomainModel dm = ((DomainModelAsset) AssetManager.Instance.findAssetByClass("DomainModelAsset")).getDomainModel();
            dm.print();
            log("End Test 2");
        }

        /// <summary>
        /// Testcase for testing implementation of update procedure with additional information
        /// </summary>
        private void performTest3()
        {
            log("Start test 3");
            CompetenceAssessmentAsset caa = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
            double transitionProbability = ((CompetenceAssessmentAssetSettings)caa.Settings).TransitionProbability;
            log("Transition Probability: " + transitionProbability);

            //setting up test environment
            setTestEnvironment3("2000", "false", "true");
            /*
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C1"), transitionProbability * (1.2));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C2"), transitionProbability * (1.1));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C3"), transitionProbability * (0.9));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C4"), transitionProbability * (0.8));
            */

            //perform update
            printCS();
            List<String> compList = new List<string>();
            List<Boolean> evidenceList = new List<Boolean>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            compList.Add("C4");
            evidenceList.Add(false);
            evidencePowers.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState(compList, evidenceList, evidencePowers);
            printCS();
            
            log("End test 3");
        }

        /// <summary>
        /// Method for setting asset parameter to default-test values
        /// </summary>
        private void setTestEnvironment3(string xi, string minonecompetence, string maxonelevel)
        {
            //create DomainModel
            DomainModel dm = new DomainModel();
            //Competences
            Elements elements = new Elements();
            CompetenceList cl = new CompetenceList();
            CompetenceDesc cd1 = new CompetenceDesc("C1");
            CompetenceDesc cd2 = new CompetenceDesc("C2");
            CompetenceDesc cd3 = new CompetenceDesc("C3");
            CompetenceDesc cd4 = new CompetenceDesc("C4");
            CompetenceDesc[] cdArray = { cd1, cd2, cd3, cd4 };
            List<CompetenceDesc> cdList = new List<CompetenceDesc>(cdArray);
            cl.competenceList = cdList;
            elements.competences = cl;
            dm.elements = elements;
            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C2", new String[] { "C1" });
            CompetenceP cp2 = new CompetenceP("C3", new String[] { "C2" });
            CompetenceP cp3 = new CompetenceP("C4", new String[] { "C3" });
            CompetenceP[] cpArray = { cp1, cp2, cp3 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;
            dm.relations = relations;
            //Update Levels
            UpdateLevel ul1 = new UpdateLevel();
            ul1.direction = "up";
            ul1.power = "low";
            ul1.xi = xi;
            ul1.minonecompetence = minonecompetence;
            ul1.maxonelevel = maxonelevel;
            UpdateLevel ul2 = new UpdateLevel();
            ul2.direction = "up";
            ul2.power = "medium";
            ul2.xi = xi;
            ul2.minonecompetence = minonecompetence;
            ul2.maxonelevel = maxonelevel;
            UpdateLevel ul3 = new UpdateLevel();
            ul3.direction = "up";
            ul3.power = "high";
            ul3.xi = xi;
            ul3.minonecompetence = minonecompetence;
            ul3.maxonelevel = maxonelevel;
            UpdateLevel ul4 = new UpdateLevel();
            ul4.direction = "down";
            ul4.power = "low";
            ul4.xi = xi;
            ul4.minonecompetence = minonecompetence;
            ul4.maxonelevel = maxonelevel;
            UpdateLevel ul5 = new UpdateLevel();
            ul5.direction = "down";
            ul5.power = "medium";
            ul5.xi = xi;
            ul5.minonecompetence = minonecompetence;
            ul5.maxonelevel = maxonelevel;
            UpdateLevel ul6 = new UpdateLevel();
            ul6.direction = "down";
            ul6.power = "high";
            ul6.xi = xi;
            ul6.minonecompetence = minonecompetence;
            ul6.maxonelevel = maxonelevel;
            UpdateLevel[] ulArray = { ul1, ul2, ul3, ul4, ul5, ul6 };
            dm.updateLevels = new UpdateLevels();
            dm.updateLevels.updateLevelList = new List<UpdateLevel>(ulArray);


            //set needed structures for asset functionality
            setDomainModel(dm);
        }

        /// <summary>
        /// Testing updates for game situations
        /// </summary>
        private void performTest4()
        {
            log("Start test 4");
            setDomainModel(createExampleDomainModel());

            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs2", true);
            printCS();

            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs6", true);
            printCS();

            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs10", false);
            printCS();

            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs99", false);
            printCS();

            log("End test 4");
        }

        /// <summary>
        /// Testing updates for activities
        /// </summary>
        private void performTest5()
        {
            log("Start test 5");
            setDomainModel(createExampleDomainModel());

            printCS();
            getCAA().updateCompetenceStateAccordingToActivity("activity1");
            printCS();

            printCS();
            getCAA().updateCompetenceStateAccordingToActivity("a4");
            printCS();

            printCS();
            getCAA().updateCompetenceStateAccordingToActivity("a2");
            printCS();

            log("End test 5");
        }

        /// <summary>
        /// Testing the game storage asset server features
        /// </summary>
        private void performTest6()
        {
            log("Start Test 6");

            setDomainModel(createExampleDomainModel());
            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs2", true);
            printCS();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs10", true);
            printCS();

            log("End Test 6");
        }

        /// <summary>
        /// Test game storage asset integration
        /// </summary>
        private void performTest7()
        {
            log("Start Test 7");

            String model = "test&uzhguZkr&We1";
            GameStorageClientAsset gameStorage;

            //setting up the game storage
            gameStorage = new GameStorageClientAsset();
            gameStorage.Bridge = AssetManager.Instance.Bridge;
            GameStorageClientAssetSettings gscas = new GameStorageClientAssetSettings();
            gscas.A2Port = 3000;
            gscas.Port = 3400;
            gscas.Host = "192.168.222.166";
            gscas.BasePath = "/api/";
            gscas.Secure = false;
            //gscas.UserToken = "a";
            gameStorage.Settings = gscas;

            gameStorage.AddModel(model);
            gameStorage[model].AddChild("t1", StorageLocations.Server);
            gameStorage[model]["t1"].Value = 0.2;
            log("stored data:" + gameStorage[model]["t1"].Value);


            //storing the data
            if (gameStorage.CheckHealth())
            {
                log(gameStorage.Health);

                if (gameStorage.Login("student", "student"))
                {
                    log("Logged in");
                }
            }


            if (gameStorage.Connected)
            {
                gameStorage.SaveStructure(model, StorageLocations.Server);
                gameStorage.SaveData(model, StorageLocations.Server, SerializingFormat.Json);
            }

            gameStorage[model].Clear();

            //loading data from the server
            if (gameStorage.CheckHealth())
            {
                log(gameStorage.Health);

                if (gameStorage.Login("student", "student"))
                {
                    log("Logged in");
                }
            }
            if (gameStorage.Connected)
            {
                gameStorage.LoadStructure(model, StorageLocations.Server);

                gameStorage.LoadData(model, StorageLocations.Server, SerializingFormat.Json);

                foreach (Node node in gameStorage[model].Children)
                {
                    Console.WriteLine("{0} {1} = {2}", node.Value.GetType().Name, node.Name, node.Value);
                }
            }

            //prompting loaded data
            log("loaded data:" + gameStorage[model]["t1"].Value);

            log("End Test 7");
        }

        /// <summary>
        /// Test tracker functionality
        /// </summary>
        private void performTest8()
        {
            log("Start Test 8");

            TrackerAsset ta = TrackerAsset.Instance;
            TrackerAssetSettings tas = new TrackerAssetSettings();
            tas.BasePath = "/api/";
            tas.Host = "192.168.222.166";
            tas.TrackingCode = "5784a7c1e8c85f6e00fab465gdj3utijicin3ik9";  //from the game
            //tas.UserToken = "a";
            tas.Secure = false;
            tas.Port = 3000;
            tas.StorageType = TrackerAsset.StorageTypes.net;
            tas.TraceFormat = TrackerAsset.TraceFormats.json;
            ta.Settings = tas;

            if (ta.CheckHealth())
            {
                log(ta.Health);
                if (ta.Login("student", "student"))
                {
                    log("logged in - tracker");
                }
            }

            if (ta.Connected)
            {
                ta.Start();
                ta.setVar("C1", "0.13");
                ta.Completable.Completed("testid");
                ta.setVar("C1", "0.33");
                ta.Completable.Completed("testid");
                ta.Flush();
            }
            else
            {
                log("Not connected to tracker.");
            }


            log("End Test 8");
        }

        #endregion Testmethods
    }

    class TestCompetenceBasedAdaptationAsset
    {
        #region HelperMethods

        /// <summary>
        /// Logging functionality for the Competence based Adaption Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            ILog logger = (ILog)AssetManager.Instance.Bridge;
            logger.Log(severity, "[CbAA Test]" + msg);
        }

        /// <summary>
        /// Method returning the Competence based Adaption Asset
        /// </summary>
        /// <returns> The Doamin Model Asset</returns>
        public CompetenceBasedAdaptionAsset getCbAA()
        {
            return (CompetenceBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("CompetenceBasedAdaptionAsset");
        }

        /// <summary>
        /// Method creating an example domain model
        /// </summary>
        /// <returns></returns>
        public DomainModel createExampleDomainModel()
        {

            DomainModel dm = new DomainModel();


            //Competences
            Elements elements = new Elements();
            CompetenceList cl = new CompetenceList();
            CompetenceDesc cd1 = new CompetenceDesc("C1");
            CompetenceDesc cd2 = new CompetenceDesc("C2");
            CompetenceDesc cd3 = new CompetenceDesc("C3");
            CompetenceDesc cd4 = new CompetenceDesc("C4");
            CompetenceDesc cd5 = new CompetenceDesc("C5");
            CompetenceDesc cd6 = new CompetenceDesc("C6");
            CompetenceDesc cd7 = new CompetenceDesc("C7");
            CompetenceDesc cd8 = new CompetenceDesc("C8");
            CompetenceDesc cd9 = new CompetenceDesc("C9");
            CompetenceDesc cd10 = new CompetenceDesc("C10");
            CompetenceDesc[] cdArray = { cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10 };
            List<CompetenceDesc> cdList = new List<CompetenceDesc>(cdArray);
            cl.competenceList = cdList;
            elements.competences = cl;

            //Game situations
            SituationsList sl = new SituationsList();
            Situation s1 = new Situation("gs1");
            Situation s2 = new Situation("gs2");
            Situation s3 = new Situation("gs3");
            Situation s4 = new Situation("gs4");
            Situation s5 = new Situation("gs5");
            Situation s6 = new Situation("gs6");
            Situation s7 = new Situation("gs7");
            Situation s8 = new Situation("gs8");
            Situation s9 = new Situation("gs9");
            Situation s10 = new Situation("gs10");
            Situation[] sArray = { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 };
            List<Situation> loList = new List<Situation>(sArray);
            sl.situationList = loList;
            elements.situations = sl;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", new String[] { "C1", "C2" });
            CompetenceP cp3 = new CompetenceP("C6", new String[] { "C4" });
            CompetenceP cp4 = new CompetenceP("C7", new String[] { "C4" });
            CompetenceP cp5 = new CompetenceP("C8", new String[] { "C3", "C6" });
            CompetenceP cp7 = new CompetenceP("C9", new String[] { "C5", "C8" });
            CompetenceP cp8 = new CompetenceP("C10", new String[] { "C9", "C7" });
            CompetenceP[] cpArray = { cp1, cp3, cp4, cp5, cp7, cp8 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            SituationRelationList lorl = new SituationRelationList();
            SituationRelation lor1 = new SituationRelation("gs1", new String[] { "C1" });
            SituationRelation lor2 = new SituationRelation("gs2", new String[] { "C2" });
            SituationRelation lor3 = new SituationRelation("gs3", new String[] { "C3" });
            SituationRelation lor4 = new SituationRelation("gs4", new String[] { "C4" });
            SituationRelation lor5 = new SituationRelation("gs5", new String[] { "C5", "C1", "C2" });
            SituationRelation lor8 = new SituationRelation("gs6", new String[] { "C6", "C4" });
            SituationRelation lor10 = new SituationRelation("gs7", new String[] { "C4", "C7" });
            SituationRelation lor12 = new SituationRelation("gs8", new String[] { "C8", "C6", "C3" });
            SituationRelation lor15 = new SituationRelation("gs9", new String[] { "C9", "C5", "C8" });
            SituationRelation lor18 = new SituationRelation("gs10", new String[] { "C10", "C9", "C7" });
            SituationRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5, lor8, lor10, lor12, lor15, lor18 };
            List<SituationRelation> lorList = new List<SituationRelation>(lorArray);
            lorl.situations = lorList;
            relations.situations = lorl;

            dm.elements = elements;
            dm.relations = relations;

            //Update Levels
            UpdateLevel ul1 = new UpdateLevel();
            ul1.direction = "up";
            ul1.power = "low";
            ul1.xi = "1.2";
            ul1.minonecompetence = "false";
            ul1.maxonelevel = "true";
            UpdateLevel ul2 = new UpdateLevel();
            ul2.direction = "up";
            ul2.power = "medium";
            ul2.xi = "2";
            ul2.minonecompetence = "false";
            ul2.maxonelevel = "true";
            UpdateLevel ul3 = new UpdateLevel();
            ul3.direction = "up";
            ul3.power = "high";
            ul3.xi = "4";
            ul3.minonecompetence = "true";
            ul3.maxonelevel = "false";
            UpdateLevel ul4 = new UpdateLevel();
            ul4.direction = "down";
            ul4.power = "low";
            ul4.xi = "1.2";
            ul4.minonecompetence = "false";
            ul4.maxonelevel = "true";
            UpdateLevel ul5 = new UpdateLevel();
            ul5.direction = "down";
            ul5.power = "medium";
            ul5.xi = "2";
            ul5.minonecompetence = "false";
            ul5.maxonelevel = "true";
            UpdateLevel ul6 = new UpdateLevel();
            ul6.direction = "down";
            ul6.power = "high";
            ul6.xi = "4";
            ul6.minonecompetence = "true";
            ul6.maxonelevel = "false";
            UpdateLevel[] ulArray = { ul1, ul2, ul3, ul4, ul5, ul6 };
            dm.updateLevels = new UpdateLevels();
            dm.updateLevels.updateLevelList = new List<UpdateLevel>(ulArray);

            return dm;
        }
        
        /// <summary>
        /// Creates an example game situation structure for testing.
        /// </summary>
        /// 
        /// <returns> An example game situation sturucture. </returns>
        private GameSituationStructure createExampleGSS()
        {
            GameSituationStructure gss = new GameSituationStructure();
            GameSituation gs1 = new GameSituation("gs1");
            GameSituation gs2 = new GameSituation("gs2");
            GameSituation gs3 = new GameSituation("gs3");
            GameSituation gs4 = new GameSituation("gs4");
            GameSituation gs5 = new GameSituation("gs5");
            GameSituation gs6 = new GameSituation("gs6");
            GameSituation gs7 = new GameSituation("gs7");
            GameSituation gs8 = new GameSituation("gs8");
            GameSituation gs9 = new GameSituation("gs9");
            GameSituation gs10 = new GameSituation("gs10");

            //define competences included in gs1
            String[] c1Array = { "C1" };
            List<String> c1 = new List<String>(c1Array);
            //define gs which are possible successors of gs1
            GameSituation[] gs1Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs1L = new List<GameSituation>(gs1Array);
            gs1.Id = "gs1";
            gs1.Successors = gs1L;
            gs1.Competences = c1;

            //define competences included in gs2
            String[] c2Array = { "C2" };
            List<String> c2 = new List<String>(c2Array);
            //define gs which are possible successors of gs2
            GameSituation[] gs2Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs2L = new List<GameSituation>(gs2Array);
            gs2.Id = "gs2";
            gs2.Successors = gs2L;
            gs2.Competences = c2;

            //define competences included in gs3
            String[] c3Array = { "C3" };
            List<String> c3 = new List<String>(c3Array);
            //define gs which are possible successors of gs3
            GameSituation[] gs3Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs3L = new List<GameSituation>(gs3Array);
            gs3.Id = "gs3";
            gs3.Successors = gs3L;
            gs3.Competences = c3;

            //define competences included in gs4
            String[] c4Array = { "C4" };
            List<String> c4 = new List<String>(c4Array);
            //define gs which are possible successors of gs4
            GameSituation[] gs4Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs4L = new List<GameSituation>(gs4Array);
            gs4.Id = "gs4";
            gs4.Successors = gs4L;
            gs4.Competences = c4;

            //define competences included in gs5
            String[] c5Array = { "C1", "C2", "C5" };
            List<String> c5 = new List<String>(c5Array);
            //define gs which are possible successors of gs5
            GameSituation[] gs5Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs5L = new List<GameSituation>(gs5Array);
            gs5.Id = "gs5";
            gs5.Successors = gs5L;
            gs5.Competences = c5;

            //define competences included in gs6
            String[] c6Array = { "C4", "C6" };
            List<String> c6 = new List<String>(c6Array);
            //define gs which are possible successors of gs6
            GameSituation[] gs6Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs6L = new List<GameSituation>(gs6Array);
            gs6.Id = "gs6";
            gs6.Successors = gs6L;
            gs6.Competences = c6;

            //define competences included in gs7
            String[] c7Array = { "C4", "C7" };
            List<String> c7 = new List<String>(c7Array);
            //define gs which are possible successors of gs7
            GameSituation[] gs7Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs7L = new List<GameSituation>(gs7Array);
            gs7.Id = "gs7";
            gs7.Successors = gs7L;
            gs7.Competences = c7;

            //define competences included in gs8
            String[] c8Array = { "C3", "C6", "C8" };
            List<String> c8 = new List<String>(c8Array);
            //define gs which are possible successors of gs8
            GameSituation[] gs8Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs8L = new List<GameSituation>(gs8Array);
            gs8.Id = "gs8";
            gs8.Successors = gs8L;
            gs8.Competences = c8;

            //define competences included in gs9
            String[] c9Array = { "C5", "C8", "C9" };
            List<String> c9 = new List<String>(c9Array);
            //define gs which are possible successors of gs9
            GameSituation[] gs9Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs9L = new List<GameSituation>(gs9Array);
            gs9.Id = "gs9";
            gs9.Successors = gs9L;
            gs9.Competences = c9;

            //define competences included in gs10
            String[] c10Array = { "C7", "C9", "C10" };
            List<String> c10 = new List<String>(c10Array);
            //define gs which are possible successors of gs1
            GameSituation[] gs10Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs10L = new List<GameSituation>(gs10Array);
            gs10.Id = "gs10";
            gs10.Successors = gs10L;
            gs10.Competences = c10;

            GameSituation[] gsArray = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gsList = new List<GameSituation>(gsArray);
            gss.GameSituations = gsList;
            gss.InitialGameSituation = gs1;

            return gss;
        }

        /// <summary>
        /// Method for setting the loaded domain model
        /// </summary>
        /// <param name="dm"> The domain model to load</param>
        public void setDomainModel(DomainModel dm)
        {
            string fileId = "CompetenceAssessmentAssetTestId.xml";

            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            //change Settings to load local file
            DomainModelAssetSettings newDMAS = new DomainModelAssetSettings();
            newDMAS.LocalSource = true;
            newDMAS.Source = fileId;
            ((DomainModelAsset)AssetManager.Instance.findAssetByClass("DomainModelAsset")).Settings = newDMAS;
        }
        
        #endregion HelperMethods
        #region TestMethods

        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            log("Competence recommendation asset tests called: ");
            performTest1();
            performTest2();
            log("Competence recommendation asset tests finished. ");
        }

        /// <summary>
        /// Creates a gamestructure and competence state/structure for performing some updates.
        /// </summary>
        private void performTest1()
        {
            log("Start Test 1");
            GameSituationStructure gss = createExampleGSS();
            gss.diagnosticPrint();
            log("End Test 1");
        }

        /// <summary>
        /// Test method - perform updates according to console input
        /// </summary>
        private void performTest2()
        {
            log("Start Test 2");
            

            List<String> userInputSimulation = new List<string>(new String[] { "s", "s", "s", "f", "s", "f", "f", "s", "f", "f", "s", "s", "s", "s", "s", "s", "s", "e" });

            setDomainModel(createExampleDomainModel());
            //GameSituationStructure gss = getGameSituationStructure();
            String gs = getCbAA().getCurrentGameSituationId();
            Dictionary<String, double> cs = ((CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset")).getCompetenceState();
            String str = "";
            foreach (KeyValuePair<String, double> pair in cs)
                str += "(" + pair.Key + ":" + Math.Round(pair.Value, 2) + ")";
            log(str);

            string line = "";
            while (gs != null)
            {
                while (!line.Equals("s") && !line.Equals("f") && !line.Equals("e"))
                {
                    //log("Entering game situation " + gs + ". How did the player performe (s-success,f-failure,e-exit)?");
                    //line = Console.ReadLine();
                    line = userInputSimulation[0];
                }
                if (!line.Equals("e"))
                {
                    log("current gs:" + getCbAA().getCurrentGameSituationId());
                    if (line.Equals("s"))
                        getCbAA().setGameSituationUpdate(true);
                    else if (line.Equals("f"))
                        getCbAA().setGameSituationUpdate(false);
                    cs = ((CompetenceAssessmentAsset) AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset")).getCompetenceState();
                    str = "";
                    foreach (KeyValuePair<String, double> pair in cs)
                        str += "(" + pair.Key + ":" + Math.Round(pair.Value, 2) + ")";
                    log(str);
                }
                else
                {
                    log("Test Ended.");
                    return;
                }
                line = "";
                gs = getCbAA().getNextGameSituationId();
                userInputSimulation.RemoveAt(0);
            }
            log("Games end reached!");
            log("End Test 2");
        }
        
        #endregion Testmethods
    }

    class Bridge : IBridge, /*IVirtualProperties,*/ ILog, IDataStorage, IWebServiceRequest, ISerializer
    {
        string IDataStoragePath = @"C:\Users\mmaurer\Desktop\rageCsFiles\";

        #region IDataStorage

        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
#warning Change DataStorage-path if needed in Program.cs, Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
            return (File.Exists(filePath));
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
#warning Change Loading-path if needed in Program.cs, Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
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
                Console.WriteLine("Error by loading the DM! - Maybe you need to change the path: \"" + IDataStoragePath + "\"");
            }

            return (null);
        }

        public void Save(string fileId, string fileData)
        {
#warning Change Saving-path if needed in Program.cs, Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
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

        /*
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
        {
            string url = requestSettings.uri.AbsoluteUri;

            if (string.Equals(requestSettings.method, "get", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);
                    HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                    Stream resStream = webResponse.GetResponseStream();

                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in webResponse.Headers.AllKeys)
                        responseHeader.Add(key, webResponse.Headers[key]);

                    StreamReader reader = new StreamReader(resStream);
                    string dm = reader.ReadToEnd();

                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)webResponse.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = dm;
                    requestResponse.body = dm;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error,"Exception: " + e.Message);
                }
            }
            else if (string.Equals(requestSettings.method, "post", StringComparison.CurrentCultureIgnoreCase))
            { //http://stackoverflow.com/questions/4015324/http-request-with-post
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

                    var data = Encoding.ASCII.GetBytes(requestSettings.body);

                    request.Method = "POST";
                    //request.ContentType = "text/plain";
                    request.ContentType = "application/json";
                    //request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = requestSettings.body;

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    
                    
                    var responsePost = (HttpWebResponse)request.GetResponse();
                    
                    var responseString = new StreamReader(responsePost.GetResponseStream()).ReadToEnd();
                    

                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in responsePost.Headers.AllKeys)
                        responseHeader.Add(key, responsePost.Headers[key]);
                    

                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)responsePost.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = responsePost.StatusDescription;
                    requestResponse.body = responseString;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error, "Exception: " +e.Message);
                    
                }
            }
            else if (string.Equals(requestSettings.method, "put", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {

                    var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);
                    request.ContentType = "text/json";
                    request.Method = "PUT";
                    request.ContentLength = Encoding.ASCII.GetBytes(requestSettings.body).Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = requestSettings.body;

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    var responseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();


                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in httpResponse.Headers.AllKeys)
                        responseHeader.Add(key, httpResponse.Headers[key]);


                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)httpResponse.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = httpResponse.StatusDescription;
                    requestResponse.body = responseString;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error, "Exception: " + e.Message);

                }
            }
            else
            {
                requestResponse = new RequestResponse();
                requestResponse.method = requestSettings.method;
                requestResponse.requestHeaders = requestSettings.requestHeaders;
                requestResponse.responsMessage = "FAIL request type unknown";
                requestResponse.uri = requestSettings.uri;
                Log(Severity.Error,"request type unknown!");
            }
        }
        */
        #endregion IWebServiceRequest
        #region ISerializer

        public bool Supports(SerializingFormat format)
        {
            switch (format)
            {
                //case SerializingFormat.Binary:
                //    return false;
                case SerializingFormat.Xml:
                    return false;
                case SerializingFormat.Json:
                    return true;
            }

            return false;
        }

        public object Deserialize<T>(string text, SerializingFormat format)
        {
            return JsonConvert.DeserializeObject(text, typeof(T));
        }

        public string Serialize(object obj, SerializingFormat format)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        #endregion ISerializer
        /*
        #region IVirtualProperties Members

        /// <summary>
        /// Looks up a given key to find its associated value.
        /// </summary>
        ///
        /// <param name="model"> The model. </param>
        /// <param name="key">   The key. </param>
        ///
        /// <returns>
        /// An Object.
        /// </returns>
        public object LookupValue(string model, string key)
        {
            if (key.Equals("Virtual"))
            {
                return DateTime.Now;
            }

            return null;
        }

        #endregion IVirtualProperties Members
        */
        #region IWebServiceRequest Members

        // See http://stackoverflow.com/questions/12224602/a-method-for-making-http-requests-on-unity-ios
        // for persistence.
        // See http://18and5.blogspot.com.es/2014/05/mono-unity3d-c-https-httpwebrequest.html

#if ASYNC
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestReponse)
        {
            // Wrap the actual method in a Task. Neccesary because we cannot:
            // 1) Make this method async (out is not allowed) 
            // 2) Return a Task<RequestResponse> as it breaks the interface (only void does not break it).
            //
            Task<RequestResponse> taskName = Task.Factory.StartNew<RequestResponse>(() =>
            {
                return WebServiceRequestAsync(requestSettings).Result;
            });

            requestReponse = taskName.Result;
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings"> Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private async Task<RequestResponse> WebServiceRequestAsync(RequestSetttings requestSettings)
#else
        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">  Options for controlling the operation. </param>
        /// <param name="requestResponse"> The request response. </param>
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
        {
            requestResponse = WebServiceRequest(requestSettings);
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private RequestResponse WebServiceRequest(RequestSetttings requestSettings)
#endif
        {
            RequestResponse result = new RequestResponse(requestSettings);

            try
            {
                //! Might throw a silent System.IOException on .NET 3.5 (sync).
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

                request.Method = requestSettings.method;

                // Both Accept and Content-Type are not allowed as Headers in a HttpWebRequest.
                // They need to be assigned to a matching property.

                if (requestSettings.requestHeaders.ContainsKey("Accept"))
                {
                    request.Accept = requestSettings.requestHeaders["Accept"];
                }

                if (!String.IsNullOrEmpty(requestSettings.body))
                {
                    byte[] data = Encoding.UTF8.GetBytes(requestSettings.body);

                    if (requestSettings.requestHeaders.ContainsKey("Content-Type"))
                    {
                        request.ContentType = requestSettings.requestHeaders["Content-Type"];
                    }

                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }

                    request.ContentLength = data.Length;

                    // See https://msdn.microsoft.com/en-us/library/system.net.servicepoint.expect100continue(v=vs.110).aspx
                    // A2 currently does not support this 100-Continue response for POST requets.
                    request.ServicePoint.Expect100Continue = false;

#if ASYNC
                    Stream stream = await request.GetRequestStreamAsync();
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Close();
#else
                    Stream stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
#endif
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

#if ASYNC
                WebResponse response = await request.GetResponseAsync();
#else
                WebResponse response = request.GetResponse();
#endif
                if (response.Headers.HasKeys())
                {
                    foreach (string key in response.Headers.AllKeys)
                    {
                        result.responseHeaders.Add(key, response.Headers.Get(key));
                    }
                }

                result.responseCode = (int)(response as HttpWebResponse).StatusCode;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
#if ASYNC
                    result.body = await reader.ReadToEndAsync();
#else
                    result.body = reader.ReadToEnd();
#endif
                }
            }
            catch (Exception e)
            {
                result.responsMessage = e.Message;

                Log(Severity.Error, String.Format("{0} - {1}", e.GetType().Name, e.Message));
            }

            return result;
        }

        #endregion IWebServiceRequest Members
    }
}
