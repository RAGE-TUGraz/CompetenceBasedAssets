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
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetenceAssessmentAssetNameSpace
{
    /// <summary>
    /// Singelton Class for handling Competence Assessment
    /// </summary>
    internal class CompetenceAssessmentHandler
    {
        #region AlgorithmParameters

        /// <summary>
        /// Algorithm variable for upgrading probabilities.
        /// </summary>
        internal double xi0 = 2;

        /// <summary>
        /// Algorithm variable for downgrading probabilities.
        /// </summary>
        internal double xi1 = 2;

        /// <summary>
        /// epsilon used for mantaining competence structure consistency 
        /// </summary>
        internal double epsilon = 0.000000001;

        /// <summary>
        /// Limit: Probabilities equal or higher as this value are assumed to indicate mastery of a competence by a learner 
        /// </summary>
        public double transitionProbability = 0.7;

        #endregion AlgorithmParameters

        //TODO: store/load competence state somewhere else
        #region Fields

        /// <summary>
        /// Instance of the DomainModelAsset
        /// </summary>
        private DomainModelAsset domainModelAsset = null;


        /// <summary>
        /// Instance of the CompetenceAssessmentAsset
        /// </summary>
        private CompetenceAssessmentAsset competenceAssessmentAsset = null;

        /// <summary>
        /// Instance of the CompetenceAssessmentHandler - Singelton pattern
        /// </summary>
        private static CompetenceAssessmentHandler instance;

        /// <summary>
        /// Dictionary containing all key/value pairs of playerId and competence structure.
        /// </summary>
        private CompetenceStructure competenceStructure = null;

        /// <summary>
        /// Structure containg the mapping between in-game activities and the related competence updates
        /// </summary>
        internal ActivityMapping activityMapping = null;

        /// <summary>
        /// Structure containg the mapping between game situations and the related competence updates for success/failure
        /// </summary>
        internal GameSituationMapping gameSituationMapping = null;

        /// <summary>
        /// Structure storing the possible update properties/powers within the asset
        /// </summary>
        internal UpdateLevelStorage updateLevelStorage = null;

        /// <summary>
        /// Dictinary containing the current competence states.
        /// </summary>
        private CompetenceState competenceState = null;

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private Boolean doLogging = true;

        #endregion Fields
        #region Constructors 

        /// <summary>
        /// Private ctor - Singelton pattern
        /// </summary>
        private CompetenceAssessmentHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the CompetenceAssessmentHandler - Singelton pattern
        /// </summary>
        public static CompetenceAssessmentHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CompetenceAssessmentHandler();
                }
                return instance;
            }
        }

        /// <summary>
        /// If set to true - logging is done, otherwise no logging is done.
        /// </summary>
        public Boolean DoLogging
        {
            set
            {
                doLogging = value;
            }
            get
            {
                return doLogging;
            }
        }

        #endregion Properties
        #region InternalMethods

        /// <summary>
        /// Method returning an instance of the DomainModelAsset.
        /// </summary>
        /// <returns> Instance of the DomainModelAsset </returns>
        internal DomainModelAsset getDMA()
        {
            if (domainModelAsset == null)
                domainModelAsset = (DomainModelAsset)AssetManager.Instance.findAssetByClass("DomainModelAsset");
            return (domainModelAsset);
        }

        /// <summary>
        /// Method returning an instance of the CompetenceAssessmentAsset.
        /// </summary>
        /// <returns> Instance of the CompetenceAssessmentAsset </returns>
        internal CompetenceAssessmentAsset getCAA()
        {
            if (competenceAssessmentAsset == null)
                competenceAssessmentAsset = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
            return (competenceAssessmentAsset);
        }

        /// <summary>
        /// Method for creating a competence-structure with an id (= playerId).
        /// </summary>
        /// 
        /// <param name="dm"> Specifies Domainmodel used to create the competence-structure. </param>
        ///
        /// <returns>
        /// Competence-structure according to the specified domainmodel.
        /// </returns>
        internal CompetenceStructure createCompetenceStructure(DomainModel dm)
        {
            if (competenceStructure != null)
                loggingCA("CompetenceStructure already exists - overwrite!");

            CompetenceStructure cst = new CompetenceStructure(dm);
            competenceStructure = cst;
            return (cst);
        }

        /// <summary>
        /// Method for creating a competence state.
        /// </summary>
        /// 
        /// <param name="cst"> Specifies competence-structure this competence state is created for. </param>
        ///
        /// <returns>
        /// Competence state according to the specified competence-structure.
        /// </returns>
        internal CompetenceState createCompetenceState(CompetenceStructure cst)
        {
            if (competenceState != null)
                loggingCA("Competence state already exists! Create new one.");

            CompetenceState cs = new CompetenceState(cst);
            competenceState = cs;
            return cs;
        }

        /// <summary>
        /// Method for performing all neccessary operations to run update methods.
        /// </summary>
        /// 
        /// <param name="dm"> Specifies the domain model used for the following registration. </param>
        internal void registerNewPlayer(DomainModel dm)
        {
            CompetenceStructure cstr = createCompetenceStructure(dm);
            createCompetenceState(cstr);
            this.updateLevelStorage = new UpdateLevelStorage(dm);
            this.gameSituationMapping = new GameSituationMapping(dm);
            this.activityMapping = new ActivityMapping(dm);
        }

        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id for the update - specifies for which player the competence state gets updated. </param>
        /// <param name="compList"> List of Strings - each String describes a competence.  </param>
        /// <param name="evidenceList"> Specifies if the evidences are speaking for or against the competence. </param>
        /// <param name="evidencePowers"> Contains the power of the evidence (Low,Medium,High) </param>
        internal void updateCompetenceState(List<String> compList, List<Boolean> evidenceList, List<EvidencePower> evidencePowers)
        {
            //ATTENTION: when updating more than one competence -> take mean - xi-limits may not work! [maybe replace mean by max/min]
            
            for (int i = 0; i < compList.Count; i++)
            {
                string evi = evidenceList[i] ? "up" : "down";
                string power = (evidencePowers[i] == EvidencePower.Low) ? "low" : (evidencePowers[i] == EvidencePower.Medium) ? "medium" : "high";
                loggingCA("updating " + compList[i] + ":" + evi+" ("+power+")");
            }

            if (competenceState == null)
            {
                loggingCA("ERROR: There is no competence state persistent!");
                return;
            }

            if (competenceStructure == null)
            {
                loggingCA("ERROR: There is no competence structure persistent!");
                return;
            }



            CompetenceState csta = competenceState;
            CompetenceStructure cstr = competenceStructure;
            
            cstr.updateCompetenceState(csta, compList, evidenceList,  evidencePowers);

        }

        /*
        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id for the update - specifies for which player the competence state gets updated. </param>
        /// <param name="compList"> List of Strings - each String describes a competence.  </param>
        /// <param name="evidence"> Specifies if the evidences are speaking for or against the competence. </param>
        /// 
        internal void updateCompetenceState(List<String> compList, Boolean evidence)
        {
            List<Boolean> evidenceList = new List<Boolean>();
            foreach (String str in compList)
                evidenceList.Add(evidence);
            updateCompetenceState( compList, evidenceList);
        }
        */

        /// <summary>
        /// Returns the competence state of the player.
        /// </summary>
        /// 
        /// <returns> Competence state of the player. </returns>
        internal CompetenceState getCompetenceState()
        {
            if (competenceState== null)
            {
                loggingCA("Player not associated with a competence state.");
                return null;
            }

            return competenceState;
        }

        #endregion InternalMethods
        #region TestMethods

        /// <summary>
        /// Method for diagnostic logging.
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingCA(String msg, Severity severity = Severity.Information)
        {
            if (DoLogging)
            {
                if(competenceAssessmentAsset==null)
                    competenceAssessmentAsset = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
                competenceAssessmentAsset.Log(severity, "[CAA]: " + msg);
            }
        }

        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            loggingCA("Competence assessment asset tests called: ");
            performTest1();
            performTest2();
            performTest3();
            performTest4();
            performTest5();
            loggingCA("Competence assessment asset tests finished. ");
        }

        
        /// <summary>
        /// Creates example domainmodel through the domainmodelhandler and performes some updates.
        /// </summary>
        private void performTest1()
        {
            loggingCA("Start Test 1");
            DomainModel dm = createExampleDomainModel();
            CompetenceStructure cst = createCompetenceStructure( dm);
            CompetenceState cs = createCompetenceState(cst);
            cs.print();

            //first update - upgrade
            List<String> compList = new List<string>();
            List<Boolean> evidenceList = new List<Boolean>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            compList.Add("C1");
            evidenceList.Add(true);
            evidencePowers.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState(compList, evidenceList, evidencePowers);
            getCompetenceState().print();

            //second update - downgrade
            List<String> compList2 = new List<string>();
            List<Boolean> evidenceList2 = new List<Boolean>();
            List<EvidencePower> evidencePowers2 = new List<EvidencePower>();
            compList2.Add("C1");
            evidenceList2.Add(false);
            evidencePowers2.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState( compList2, evidenceList2, evidencePowers2);
            getCompetenceState().print();
            loggingCA("End Test 1");
        }
        

        /// <summary>
        /// Test method printing out an example domain model. 
        /// </summary>
        private void performTest2()
        {
            loggingCA("Start Test 2");
            DomainModel dm = getDMA().getDomainModel();
            dm.print();
            loggingCA("End Test 2");
        }

        /// <summary>
        /// Testcase for testing implementation of update procedure with additional information
        /// </summary>
        private void performTest3()
        {
            loggingCA("Start test 3");
            loggingCA("Transition Probability: " +transitionProbability);

            //setting up test environment
            setTestEnvironment3("2000","false","true");
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C1"), this.transitionProbability * (1.2));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C2"), this.transitionProbability * (1.1));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C3"), this.transitionProbability * (0.9));
            this.competenceState.setCompetenceValue(this.competenceStructure.getCompetenceById("C4"), this.transitionProbability * (0.8));

            //perform update
            getCompetenceState().print();
            List<String> compList = new List<string>();
            List<Boolean> evidenceList = new List<Boolean>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            compList.Add("C4");
            evidenceList.Add(false);
            evidencePowers.Add(EvidencePower.Medium);
            getCAA().updateCompetenceState(compList, evidenceList, evidencePowers);
            getCompetenceState().print();
            getCompetenceState().printMasteredCompetences();

            registerNewPlayer(getDMA().getDomainModel());
            loggingCA("End test 3");
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
            this.registerNewPlayer(dm);
        }

        /// <summary>
        /// Testing updates for game situations
        /// </summary>
        private void performTest4()
        {
            loggingCA("Start test 4");
            registerNewPlayer(createExampleDomainModel());

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs2",true);
            getCompetenceState().print();

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs6", true);
            getCompetenceState().print();

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs10", false);
            getCompetenceState().print();

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToGamesituation("gs99", false);
            getCompetenceState().print();

            loggingCA("End test 4");
        }

        /// <summary>
        /// Testing updates for game situations
        /// </summary>
        private void performTest5()
        {
            loggingCA("Start test 5");
            registerNewPlayer(createExampleDomainModel());

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToActivity("activity1");
            getCompetenceState().print();

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToActivity("a4");
            getCompetenceState().print();

            getCompetenceState().print();
            getCAA().updateCompetenceStateAccordingToActivity("a2");
            getCompetenceState().print();

            loggingCA("End test 5");
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

            //activities
            ActivityList al = new ActivityList();
            Activity a1 = new Activity("a1");
            Activity a2 = new Activity("a2");
            Activity a3 = new Activity("a3");
            Activity a4 = new Activity("a4");
            Activity a5 = new Activity("a5");
            Activity[] aArray = { a1,a2,a3,a4,a5};
            List<Activity> aList = new List<Activity>(aArray);
            al.activityList = aList;
            elements.activities = al;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", new String[] { "C1","C2" });
            CompetenceP cp3 = new CompetenceP("C6", new String[] { "C4" });
            CompetenceP cp4 = new CompetenceP("C7", new String[] { "C4" });
            CompetenceP cp5 = new CompetenceP("C8", new String[] { "C3", "C6" });
            CompetenceP cp7 = new CompetenceP("C9", new String[] { "C5", "C8" });
            CompetenceP cp8 = new CompetenceP("C10", new String[] { "C9", "C7" });
            CompetenceP[] cpArray = { cp1,  cp3, cp4, cp5,  cp7, cp8 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            SituationRelationList lorl = new SituationRelationList();
            SituationRelation lor1 = new SituationRelation("gs1",new String[] { "C1" });
            SituationRelation lor2 = new SituationRelation("gs2", new String[] { "C2" });
            SituationRelation lor3 = new SituationRelation("gs3", new String[] { "C3" });
            SituationRelation lor4 = new SituationRelation("gs4", new String[] { "C4" });
            SituationRelation lor5 = new SituationRelation("gs5", new String[] { "C5", "C1", "C2" });
            SituationRelation lor8 = new SituationRelation("gs6", new String[] { "C6", "C4" });
            SituationRelation lor10 = new SituationRelation("gs7", new String[] { "C4", "C7" });
            SituationRelation lor12 = new SituationRelation("gs8", new String[] { "C8", "C6", "C3" });
            SituationRelation lor15 = new SituationRelation("gs9", new String[] { "C9", "C5", "C8" });
            SituationRelation lor18 = new SituationRelation("gs10", new String[] { "C10", "C9", "C7" });
            SituationRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5,  lor8,  lor10,  lor12,  lor15,  lor18 };
            List<SituationRelation> lorList = new List<SituationRelation>(lorArray);
            lorl.situations = lorList;
            relations.situations = lorl;

            //assignmend of competences to activities
            ActivityRelationList arl = new ActivityRelationList();
            ActivitiesRelation ar1 = new ActivitiesRelation("a1",new CompetenceActivity[] { new CompetenceActivity("C1","medium","up")});
            ActivitiesRelation ar2 = new ActivitiesRelation("a2", new CompetenceActivity[] { new CompetenceActivity("C1", "medium", "up"), new CompetenceActivity("C2", "low", "down") });
            ActivitiesRelation ar3 = new ActivitiesRelation("a3", new CompetenceActivity[] { new CompetenceActivity("C5", "low", "up"), new CompetenceActivity("C4", "high", "down") });
            ActivitiesRelation ar4 = new ActivitiesRelation("a4", new CompetenceActivity[] { new CompetenceActivity("C1", "high", "up"), new CompetenceActivity("C8", "high", "up") });
            ActivitiesRelation ar5 = new ActivitiesRelation("a5", new CompetenceActivity[] { new CompetenceActivity("C6", "low", "down"), new CompetenceActivity("C7", "medium", "up") });
            ActivitiesRelation[] arArray = {ar1,ar2,ar3,ar4,ar5 };
            List<ActivitiesRelation> arList = new List<ActivitiesRelation>(arArray);
            arl.activities = arList;
            relations.activities = arl;

            //general 
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


        #endregion TestMethods

    }

    /// <summary>
    /// Class representing the Competence-Tree of the Domainmodel.
    /// </summary>
    public class CompetenceStructure
    {
        #region Fields 

        /// <summary>
        /// Domainmodel-ID
        /// </summary>
        internal String domainModelId;

        /// <summary>
        /// List of competences forming the competence-structure
        /// </summary>
        internal List<Competence> competences = new List<Competence>();

        /// <summary>
        /// Algorithm-parameters for updating a competence state áccording to this competence-structure.
        /// </summary>
        private double xi0 = CompetenceAssessmentHandler.Instance.xi0;
        private double xi1 = CompetenceAssessmentHandler.Instance.xi1;
        private double epsilon = CompetenceAssessmentHandler.Instance.epsilon;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Constructor using a DomainModel.
        /// </summary>
        /// 
        /// <param name="dm"> DomainModel which is used to create the CompetenceStructure. </param>
        internal CompetenceStructure(DomainModel dm)
        {
            //domainModelId = dm.metadata.id;

            //adding competences
            foreach (CompetenceDesc comd in dm.elements.competences.competenceList)
            {
                competences.Add(new Competence(comd.id, comd.title, this));
            }

            //adding prerequisites and successors
            foreach (CompetenceP comp in dm.relations.competenceprerequisites.competences)
            {
                foreach(Prereqcompetence pcom in comp.prereqcompetences)
                {
                    getCompetenceById(comp.id).addPrerequisite(getCompetenceById(pcom.id));
                    getCompetenceById(pcom.id).addSuccessor(getCompetenceById(comp.id));
                }
            }


        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for getting Competence by ID from competence structure.
        /// </summary>
        /// 
        /// <param name="id"> Unique competence-id within a competence structure. </param>
        ///
        /// <returns>
        /// Competence specified by the given id.
        /// </returns>
        internal Competence getCompetenceById(String id)
        {
            foreach (Competence com in competences)
            {
                if (com.id.Equals(id))
                {
                    return (com);
                }
            }
            return (null);
        }

        /// <summary>
        /// Diagnostic-method for displaying the competence structure
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Printing competence-structure:");
            CompetenceAssessmentHandler.Instance.loggingCA("==============================");

            foreach (Competence com in competences)
            {
                com.print();
            }
        }

        /// <summary>
        /// Method for updating a competence state with a set of evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="compList"> Speciefies for which Competences evidences are observed. </param>
        /// <param name="evidenceList"> Specifies if evidences are observed for (true) or against (false) possessing a competence. </param>
        /// <param name="evidencePowers"> Algorithm parameter for updating competence probabilities -> defines xi values and update power </param>
        internal void updateCompetenceState(CompetenceState cs, List<Competence> compList, List<Boolean> evidenceList, List<EvidencePower> evidencePowers)
        {
            Dictionary<string, double> sum = new Dictionary<string, double>();

            //initialise all sum-values with zero
            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                sum[comp.id] = 0.0;
            }

            Dictionary<string, double> tmp;
            for (int i = 0; i < compList.Count; i++)
            {
                tmp = updateCompetenceStateWithOneEvidence(cs, compList[i], evidenceList[i], evidencePowers[i]);

                foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
                {
                    sum[comp.id] = sum[comp.id] + tmp[comp.id];
                }
            }

            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                cs.setCompetenceValue(comp, sum[comp.id] / compList.Count);
            }

        }

        /*
        /// <summary>
        /// Method for updating a competence state with one evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="comp"> Speciefies for which Competence evidence is observed. </param>
        /// <param name="evidence"> Specifies if evidence is observed for (true) or against (false) possessing a competence. </param>
        /// <param name="xi0List"> Algorithm parameter for updating competence probabilities. </param>
        /// <param name="xi1List"> Algorithm parameter for updating competence probabilities. </param>
        internal void updateCompetenceState(CompetenceState cs, String comp, Boolean evidence, List<double> xi0List, List<double> xi1List)
        {
            updateCompetenceState(cs, this.getCompetenceById(comp), evidence, xi0List, xi1List);
        }
        */

        /// <summary>
        /// Method for updating a competence state with a set of evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="compList"> Speciefies for which Competences (by id) evidences are observed. </param>
        /// <param name="evidenceList"> Specifies if evidences are observed for (true) or against (false) possessing a competence. </param>
        /// <param name="xi0List"> Algorithm parameter for updating competence probabilities. </param>
        /// <param name="xi1List"> Algorithm parameter for updating competence probabilities. </param>
        /// <param name="additionalInformation"> Specifies if updating a competence is able to get a successor-competence in the competence state or for sure removes a prerequisite competence from the competence state by modifying xi0 or xi1.</param>
        internal void updateCompetenceState(CompetenceState cs, List<String> compList, List<Boolean> evidenceList, List<EvidencePower> evidencePowers)
        {
            List<Competence> cList = new List<Competence>();
            foreach (String str in compList)
            {
                if (getCompetenceById(str) != null)
                    cList.Add(getCompetenceById(str));
            }

            updateCompetenceState(cs, cList, evidenceList, evidencePowers);
        }

        /*
        /// <summary>
        /// Method for updating a competence state with one evidence.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="comp"> Speciefies for which Competence evidence is observed. </param>
        /// <param name="evidence"> Specifies if evidence is observed for (true) or against (false) possessing a competence. </param>
        /// <param name="xi0List"> Algorithm parameter for updating competence probabilities. </param>
        /// <param name="xi1List"> Algorithm parameter for updating competence probabilities. </param>
        internal void updateCompetenceState(CompetenceState cs, Competence comp, Boolean evidence, List<double> xi0List, List<double> xi1List)
        {
            List<Competence> compList = new List<Competence>();
            compList.Add(comp);
            List<Boolean> evidenceList = new List<Boolean>();
            evidenceList.Add(evidence);
            updateCompetenceState(cs, compList, evidenceList, xi0List, xi1List);
        }
        */

        /// <summary>
        /// Method for updating a competence state with one evidence.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="com"> Specifies for which competence an evidence is available. </param>
        /// <param name="evidence"> Specifies if the evidence indicates possesion (true) of the competence or not (false). </param>
        /// <param name="newXi0"> Algorithm parameter for updating the competence-probabilities. </param>
        /// <param name="newXi1"> Algorithm parameter for updating the competence-probabilities. </param>
        /// <param name="additionalInformation"> Specifies if updating a competence is able to get a successor-competence in the competence state or for sure removes a prerequisite competence from the competence state by modifying xi0 or xi1.</param>
        ///
        /// <returns>
        /// Dictionary with key/value pairs of competence-id and updated probability of pessesing the competence. 
        /// </returns>
        internal Dictionary<string, double> updateCompetenceStateWithOneEvidence(CompetenceState cs, Competence com, Boolean evidence, EvidencePower evidencePower)
        {
            CompetenceAssessmentHandler cah = CompetenceAssessmentHandler.Instance;
            ULevel ulevel = evidence ? CompetenceAssessmentHandler.Instance.updateLevelStorage.up[evidencePower] : CompetenceAssessmentHandler.Instance.updateLevelStorage.down[evidencePower];


            Dictionary<string, double> pairs = new Dictionary<string, double>();
            Double denominator;

            //additionaInformation structure: {downgrading->lose a competence for sure?, upgrading->gaine a competence for sure?, upgrading-> is it possible to gaine more than one competence?}
            double[] updateValues = getUpdateValues(ulevel, evidence, cs, com);
            double newXi0 = updateValues[0];
            double newXi1 = updateValues[1];

            //starting the update procedure
            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                pairs[comp.id] = 0.0;
            }

            if (evidence)
                denominator = newXi0 * cs.getValue(com.id) + (1 - cs.getValue(com.id));
            else
                denominator = cs.getValue(com.id) + newXi1 * (1 - cs.getValue(com.id));

            foreach (Competence competence in this.competences)
            {
                if (com.isIndirectPrerequesiteOf(competence) && com.id != competence.id)
                {
                    if (evidence)
                        pairs[competence.id] = (newXi0 * cs.getValue(competence.id)) / denominator;
                    else
                        pairs[competence.id] = cs.getValue(competence.id) / denominator;
                }
                else if (competence.isIndirectPrerequesiteOf(com))
                {
                    if (evidence)
                        pairs[competence.id] = (newXi0 * cs.getValue(com.id) + (cs.getValue(competence.id) - cs.getValue(com.id))) / denominator;
                    else
                        pairs[competence.id] = (cs.getValue(com.id) + newXi1 * (cs.getValue(competence.id) - cs.getValue(com.id))) / denominator;
                }
                else
                {
                    pairs[competence.id] = cs.getValue(competence.id);
                }
            }

            checkConsistency(pairs, evidence);

            return (pairs);
        }

        /// <summary>
        /// Method for adapting the xi-values to the given additional information about the update
        /// </summary>
        /// <param name="ulevel"> Update information (original xi values and additional information)</param>
        /// <param name="evidence"> indicates if there is an up- or downgrade</param>
        /// <param name="cs"> competence state</param>
        /// <param name="com">competence, which gets updated</param>
        /// <returns> the adopted xi values </returns>
        private double[] getUpdateValues(ULevel ulevel, Boolean evidence, CompetenceState cs, Competence com)
        {

            List<Competence> possibleCompetencesToShiftMinOneLevel = new List<Competence>();
            Boolean isCompetenceMastered = cs.getMasteredCompetences().Contains(com);

            double newXi0 = ulevel.xi;
            double newXi1 = ulevel.xi;
            double xi0 = newXi0;
            double xi1 = newXi1;

            //add competence for minonelevel-property 
            if (evidence && (ulevel.minonecompetence || ulevel.maxonelevel))
            {
                if (!isCompetenceMastered)
                {
                    List<Competence> candidatesToShift = new List<Competence>();
                    candidatesToShift.Add(com);

                    List<Competence> prerequisitesNotMastered;
                    while (candidatesToShift.Count > 0)
                    {
                        prerequisitesNotMastered = candidatesToShift[0].getPrerequisitesNotMastered(cs);
                        if (prerequisitesNotMastered.Count == 0)
                            possibleCompetencesToShiftMinOneLevel.Add(candidatesToShift[0]);
                        else
                            foreach (Competence c in prerequisitesNotMastered)
                                candidatesToShift.Add(c);
                        candidatesToShift.RemoveAt(0);
                    }
                }
                else
                {
                    List<Competence> candidatesToGetShiftElements = new List<Competence>();
                    candidatesToGetShiftElements.Add(com);
                    while (candidatesToGetShiftElements.Count > 0)
                    {
                        foreach (Competence c in candidatesToGetShiftElements[0].successors)
                        {
                            if (cs.getMasteredCompetences().Contains(c))
                                candidatesToGetShiftElements.Add(c);
                            else
                                if (c.allPrerequisitesMet(cs))
                                possibleCompetencesToShiftMinOneLevel.Add(c);
                        }
                        candidatesToGetShiftElements.RemoveAt(0);
                    }
                }
            }
            else if ((!evidence) && (ulevel.minonecompetence || ulevel.maxonelevel))
            {
                if (!isCompetenceMastered)
                {
                    List<Competence> candidatesToGetShiftElements = new List<Competence>();
                    candidatesToGetShiftElements.Add(com);
                    while (candidatesToGetShiftElements.Count > 0)
                    {
                        foreach (Competence c in candidatesToGetShiftElements[0].prerequisites)
                        {
                            if (!cs.getMasteredCompetences().Contains(c))
                                candidatesToGetShiftElements.Add(c);
                            else
                                possibleCompetencesToShiftMinOneLevel.Add(c);
                        }
                        candidatesToGetShiftElements.RemoveAt(0);
                    }
                }
                else
                {
                    List<Competence> candidateShiftElements = new List<Competence>();
                    candidateShiftElements.Add(com);

                    List<Competence> successorsMastered;
                    while (candidateShiftElements.Count > 0)
                    {
                        successorsMastered = new List<Competence>();
                        foreach (Competence c in candidateShiftElements[0].successors)
                        {
                            if (cs.getMasteredCompetences().Contains(c))
                                successorsMastered.Add(c);
                        }
                        if (successorsMastered.Count == 0)
                            possibleCompetencesToShiftMinOneLevel.Add(candidateShiftElements[0]);
                        else
                            foreach (Competence succomp in successorsMastered)
                                candidateShiftElements.Add(succomp);
                        candidateShiftElements.RemoveAt(0);
                    }
                }
            }

            /*
            String str = "Possible competences to shift:  ";
            foreach (Competence c in possibleCompetencesToShiftMinOneLevel)
                str += c.id + ",";
            CompetenceAssessmentHandler.Instance.loggingCA(str);
            */

            //upgrading->gaine a competence for sure?
            if (ulevel.minonecompetence && evidence && possibleCompetencesToShiftMinOneLevel.Count > 0)
            {
                double lowestXiNeededForUpdate = 0;
                double currentXiNeededForUpdate;
                foreach (Competence competence in possibleCompetencesToShiftMinOneLevel)
                {
                    currentXiNeededForUpdate = competence.calculateXi(com, cs.transitionProbability + epsilon, cs, evidence);
                    if (lowestXiNeededForUpdate==0 || (lowestXiNeededForUpdate > currentXiNeededForUpdate))
                        lowestXiNeededForUpdate = currentXiNeededForUpdate;
                }
                newXi0 = Math.Max(lowestXiNeededForUpdate, newXi0);
            }

            //downgrading->lose a competence for sure?
            if (ulevel.minonecompetence && (!evidence) && possibleCompetencesToShiftMinOneLevel.Count > 0)
            {
                double lowestXiNeededForUpdate = 0;
                double currentXiNeededForUpdate;
                foreach (Competence competence in possibleCompetencesToShiftMinOneLevel)
                {
                    currentXiNeededForUpdate = competence.calculateXi(com, cs.transitionProbability - epsilon, cs, evidence);
                    if (lowestXiNeededForUpdate == 0 || (lowestXiNeededForUpdate > currentXiNeededForUpdate))
                        lowestXiNeededForUpdate = currentXiNeededForUpdate; 
                }
                newXi1 = Math.Max(lowestXiNeededForUpdate, newXi1);
            }

            //handling maxonelevel-property
            if (ulevel.maxonelevel && possibleCompetencesToShiftMinOneLevel.Count > 0)
            {
                List<Competence> possibleCompetencesToShiftMaxOneLevel = new List<Competence>();
                if (evidence)
                {
                    foreach (Competence competence in possibleCompetencesToShiftMinOneLevel)
                        foreach (Competence comp in competence.getSuccessorsWithAllPrerequisitesMasteredButThis(cs))
                            if (!possibleCompetencesToShiftMaxOneLevel.Contains(comp))
                                possibleCompetencesToShiftMaxOneLevel.Add(comp);
                }
                else
                {
                    foreach (Competence competence in possibleCompetencesToShiftMinOneLevel)
                        foreach (Competence comp in competence.getPrerequisiteWithAllSuccessorsNotInCompetenceStateButThis(cs))
                            if (!possibleCompetencesToShiftMaxOneLevel.Contains(comp))
                                possibleCompetencesToShiftMaxOneLevel.Add(comp);
                }

                /*
                String str2 = "Possible competences to shift:  ";
                foreach (Competence c in possibleCompetencesToShiftMaxOneLevel)
                    str2 += c.id + ",";
                CompetenceAssessmentHandler.Instance.loggingCA(str2);
                */

                //upgrading->gaine not more than one competence level
                if (evidence && possibleCompetencesToShiftMaxOneLevel.Count > 0)
                {
                    double maxXiAllowedForUpdate = 0;
                    double currentXiAllowedForUpdate;
                    foreach (Competence competence in possibleCompetencesToShiftMaxOneLevel)
                    {
                        currentXiAllowedForUpdate = competence.calculateXi(com, cs.transitionProbability - epsilon, cs, evidence);
                        if ((maxXiAllowedForUpdate ==0 || (maxXiAllowedForUpdate > currentXiAllowedForUpdate))&& currentXiAllowedForUpdate>1)
                            maxXiAllowedForUpdate = currentXiAllowedForUpdate;
                    }
                    newXi0 = (maxXiAllowedForUpdate > 1) ? Math.Min(maxXiAllowedForUpdate, newXi0) : newXi0;
                    //newXi0 = Math.Max(newXi0, 1 + epsilon);
                }


                //downgrading->lose not more than one competence level
                if ((!evidence) && possibleCompetencesToShiftMaxOneLevel.Count > 0)
                {
                    double maxXiAllowedForUpdate = 0;
                    double currentXiAllowedForUpdate;
                    foreach (Competence competence in possibleCompetencesToShiftMaxOneLevel)
                    {
                        currentXiAllowedForUpdate = competence.calculateXi(com, cs.transitionProbability + epsilon, cs, evidence);
                        if ((maxXiAllowedForUpdate == 0 || (maxXiAllowedForUpdate > currentXiAllowedForUpdate)) && currentXiAllowedForUpdate > 1)
                            maxXiAllowedForUpdate = currentXiAllowedForUpdate;
                    }
                    newXi1 = (maxXiAllowedForUpdate>1) ? Math.Min(maxXiAllowedForUpdate, newXi1) : newXi1;
                    //newXi1 = Math.Max(newXi1, 1 + epsilon);
                }
            }

            if (evidence && (xi0 != newXi0))
            {
                CompetenceAssessmentHandler.Instance.loggingCA("xi0 changed from " + xi0 + " to " + newXi0 + " due to additional information.");
                if (newXi0 < 1)
                    throw new Exception("Internal error Competence Assessment Asset: Value not allowed!");
            }
            else if ((!evidence) && (xi1 != newXi1))
            {
                CompetenceAssessmentHandler.Instance.loggingCA("xi1 changed from " + xi1 + " to " + newXi1 + " due to additional information.");
                if (newXi1 < 1)
                    throw new Exception("Internal error Competence Assessment Asset: Value not allowed!");
            }

            double[] updateValues = { newXi0,newXi1};
            return updateValues;
        }

        /// <summary>
        /// Method for checking (and restore) competence structure consistency - prerequisites must have a higher probability of beeing possessed.
        /// </summary>
        /// 
        /// <param name="pairs"> Dictionary with key/value pairs of competence-id and probability of pessessing the competence. </param>
        /// <param name="evidence"> Specifies if the evidence indicates possesion (true) of the competence or not (false). </param>
        private void checkConsistency(Dictionary<String, double> pairs, Boolean evidence)
        {
            Boolean changes = true;
            while (changes)
            {
                changes = false;
                foreach (Competence com1 in competences)
                {
                    foreach (Competence com2 in competences)
                    {
                        if (com1.id != com2.id && com1.isIndirectPrerequesiteOf(com2) && pairs[com1.id] <= pairs[com2.id])
                        {
                            if (evidence)
                                pairs[com1.id] = Math.Min(1 - epsilon, pairs[com2.id] + epsilon);
                            else
                                pairs[com2.id] = Math.Max(epsilon, pairs[com1.id] - epsilon);
                            changes = true;
                        }
                    }
                }
            }
        }

        #endregion Methods

    }


    /// <summary>
    /// Class representing a Competence in the Competence-Tree of the Domainmodel.
    /// </summary>
    public class Competence
    {
        #region Fields 

        /// <summary>
        /// Unique id within a competence-structure
        /// </summary>
        public string id;

        /// <summary>
        /// Human-readable name of the competence
        /// </summary>
        public string title;

        /// <summary>
        /// List of prerequisites to this competence
        /// </summary>
        public List<Competence> prerequisites = new List<Competence>();

        /// <summary>
        /// List of successors to this competence
        /// </summary>
        public List<Competence> successors = new List<Competence>();

        /// <summary>
        /// Competence-structure containing this competence
        /// </summary>
        public CompetenceStructure cst;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Competence ctor
        /// </summary>
        /// 
        /// <param name="newId"> Unique Competence Id within a competence structure. </param>
        /// <param name="newTitle"> Competence name/description. </param>
        /// <param name="newCst"> Spezifies CompetenceStructure this competence is contained in. </param>
        public Competence(String newId, String newTitle, CompetenceStructure newCst)
        {
            id = newId;
            title = newTitle;
            cst = newCst;
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for adding a prerequisite.
        /// </summary>
        /// 
        /// <param name="prerequisite"> Specifies prerequisite to be added to the competence. </param>
        public void addPrerequisite(Competence prerequisite)
        {
            prerequisites.Add(prerequisite);
        }

        /// <summary>
        /// Method for adding a successor.
        /// </summary>
        /// 
        /// <param name="successor"> Specifies successor to be added to the competence. </param>
        public void addSuccessor(Competence successor)
        {
            successors.Add(successor);
        }

        /// <summary>
        /// Diagnostic method for displaying a competence.
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Competence: " + id);

            if (prerequisites.Count > 0)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("Prerequisites: ");
            }
            foreach (Competence com in prerequisites)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("       - " + com.id);
            }

            if (successors.Count > 0)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("Successors: ");
            }
            foreach (Competence com in successors)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("       - " + com.id);
            }
        }

        /// <summary>
        /// Method determining if the competence (this) is a (in)direct prerequisite of a given competence (com).
        /// </summary>
        /// 
        /// <param name="com"> Specifies for which competence the checking is done. </param>
        ///
        /// <returns>
        /// Boolean: True if the competence (this) is a (in)direct prerequisite of a given competence (com), false otherwise.
        /// </returns>
        public Boolean isIndirectPrerequesiteOf(Competence com)
        {
            if (this.id == com.id)
            {
                return (true);
            }
            else
            {
                if (com.prerequisites.Count == 0)
                    return (false);

                foreach (Competence c in com.prerequisites)
                {
                    if (this.isIndirectPrerequesiteOf(c))
                        return (true);
                }
                return (false);
            }
        }

        /// <summary>
        /// Method determining if the competence (this) is a (in)direct successor of a given competence (com).
        /// </summary>
        /// 
        /// <param name="com"> Specifies for which competence the checking is done. </param>
        ///
        /// <returns>
        /// Boolean: True if the competence (this) is a (in)direct successor of a given competence (com), false otherwise.
        /// </returns>
        public Boolean isIndirectSuccessorOf(Competence com)
        {
            return (!com.isIndirectPrerequesiteOf(this));
        }

        /// <summary>
        /// Method returning the set of all direct prerequisites not mastered with an given competence state
        /// </summary>
        /// <param name="cs"> competence state for wich the set should be returned </param>
        /// <returns> List of not possessed direct prerequisite competences </returns>
        public List<Competence> getPrerequisitesNotMastered(CompetenceState cs)
        {
            List<Competence> prereqNotMastered = new List<Competence>();
            foreach (Competence com in this.prerequisites)
                if (cs.getValue(com.id) < cs.transitionProbability)
                    prereqNotMastered.Add(com);
            return (prereqNotMastered);
        }

        /// <summary>
        /// Method determining, if all prerequisites to one competence are met
        /// </summary>
        /// <param name="cs"> Competence for which this is determined </param>
        /// <returns> True, if all prerequisites are met, false otherwise</returns>
        public Boolean allPrerequisitesMet(CompetenceState cs)
        {
            Boolean allPrerequisitesMet = true;
            foreach(Competence com in this.prerequisites)
            {
                if (cs.getValue(com.id) < cs.transitionProbability)
                {
                    allPrerequisitesMet = false;
                    break;
                }
            }

            return allPrerequisitesMet;
        }

        /// <summary>
        /// Method for getting all successor-competence for which all prerequisites are met, but this one
        /// </summary>
        /// <param name="cs"> Competence state </param>
        /// <returns>successor-competence for which all prerequisites are met, but this one</returns>
        public List<Competence> getSuccessorsWithAllPrerequisitesMasteredButThis(CompetenceState cs)
        {
            List<Competence> successorsWithAllPrerequisitesMasteredButThis = new List<Competence>();
            foreach(Competence competence in this.successors)
            {
                List<Competence> prerequisitesNotMastered = competence.getPrerequisitesNotMastered(cs);
                if (prerequisitesNotMastered.Count == 1 && prerequisitesNotMastered[0].id.Equals(this.id))
                    successorsWithAllPrerequisitesMasteredButThis.Add(competence);
            }
            return successorsWithAllPrerequisitesMasteredButThis;
        }

        /// <summary>
        /// Method for getting all mastered successors of one competence
        /// </summary>
        /// <param name="cs"> Competence state </param>
        /// <returns>all mastered successors of one competence</returns>
        public List<Competence> getSuccessorsMastered(CompetenceState cs)
        {
            List<Competence> successorsMastered = new List<Competence>();
            foreach (Competence competence in this.successors)
                if (cs.getValue(competence.id) >= cs.transitionProbability)
                    successorsMastered.Add(competence);
            return (successorsMastered);
        }

        /// <summary>
        /// Method for getting all prerequisite competences for one competence for which non successor is mastered but this one
        /// </summary>
        /// <param name="cs"> competence state </param>
        /// <returns>all prerequisite competences for one competence for which non successor is mastered but this one</returns>
        public List<Competence> getPrerequisiteWithAllSuccessorsNotInCompetenceStateButThis(CompetenceState cs)
        {
            List<Competence> prerequisiteWithAllSuccessorsNotInCompetenceStateButThis = new List<Competence>();
            foreach (Competence competence in this.prerequisites)
            {
                List<Competence> successorsMastered = competence.getSuccessorsMastered(cs);
                if (successorsMastered.Count == 1 && successorsMastered[0].id.Equals(this.id))
                    prerequisiteWithAllSuccessorsNotInCompetenceStateButThis.Add(competence);
            }
            return prerequisiteWithAllSuccessorsNotInCompetenceStateButThis;
        }

        /// <summary>
        /// Method for calculating an update value, such that the competence reaches a certain limit
        /// </summary>
        /// <param name="updatedCompetence"> competence which gets updated </param>
        /// <param name="limitToBeReached"> the probability to be reached for this competence </param>
        /// <param name="cs"> the corresponding competence state </param>
        /// <param name="evidenceDirection"> indicates if an up- or downgrad is happening </param>
        /// <returns> the xi value for reaching the certain probability </returns>
        public double calculateXi(Competence updatedCompetence, double limitToBeReached, CompetenceState cs, Boolean evidenceDirection)
        {
            if (evidenceDirection)
            {
                if (updatedCompetence.isIndirectPrerequesiteOf(this) && updatedCompetence.id != this.id)
                {
                    return ((limitToBeReached*(1-cs.getValue(updatedCompetence.id))) /(cs.getValue(this.id)-cs.getValue(updatedCompetence.id)*limitToBeReached));
                }
                else if (this.isIndirectPrerequesiteOf(updatedCompetence))
                {
                    return ((limitToBeReached-cs.getValue(updatedCompetence)*limitToBeReached-cs.getValue(this.id)+cs.getValue(updatedCompetence.id))/(cs.getValue(updatedCompetence.id)*(1-limitToBeReached)));
                }
                else
                    throw new Exception("This line should not be reached!");
            }
            else
            {
                if (updatedCompetence.isIndirectPrerequesiteOf(this) && updatedCompetence.id != this.id)
                {
                    return ((cs.getValue(this.id)-limitToBeReached*cs.getValue(updatedCompetence.id))/(limitToBeReached*(1-cs.getValue(updatedCompetence.id))));
                }
                else if (this.isIndirectPrerequesiteOf(updatedCompetence))
                {
                    return ((cs.getValue(updatedCompetence.id)*(limitToBeReached-1))/(-limitToBeReached*(1-cs.getValue(updatedCompetence.id))+(cs.getValue(this.id)-cs.getValue(updatedCompetence.id))));
                }
                else
                    throw new Exception("This line should not be reached!");
            }
        }

        #endregion Methods

    }

    /// <summary>
    /// Class representing the competence-state of a learner.
    /// </summary>
    public class CompetenceState
    {
        #region Fields

        /// <summary>
        /// Dictionary containing the key/value pairs of competences and probability of possession assosiated with the competence
        /// </summary>
        Dictionary<Competence, double> pairs = new Dictionary<Competence, double>();

        /// <summary>
        /// Limit: Values of CompetenceAssessmentHandler.Instance.transitionProbability and above are assumed to indicate mastery of a competence by a learner 
        /// </summary>
        public double transitionProbability = CompetenceAssessmentHandler.Instance.transitionProbability;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// CompetenceState ctor
        /// </summary>
        /// 
        /// <param name="cst"> Spezifies CompetenceStructure this CompetenceState is created for. </param>
        public CompetenceState(CompetenceStructure cst)
        {
            setInitialCompetenceState(cst);
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for accessing the current competence-state values.
        /// </summary>
        ///
        /// <returns>
        /// Dictionary containing Competences and the probability of possessing them.
        /// </returns>
        public Dictionary<Competence, double> getCurrentValues()
        {
            return (pairs);
        }

        /// <summary>
        /// Method for accessing the probability of possessing a competence.
        /// </summary>
        /// 
        /// <param name="competence"> Spezifies for which competence the probability is returned. </param>
        ///
        /// <returns>
        /// Probability of possessing the spezified competence.
        /// </returns>
        public double getValue(Competence competence)
        {
            return (pairs[competence]);
        }

        /// <summary>
        /// Method for accessing the probability of possessing a competence.
        /// </summary>
        /// 
        /// <param name="competence"> String- specifying for which competence the probability is returned. </param>
        ///
        /// <returns>
        /// Probability of possessing the specified competence - if available; -1 otherwise.
        /// </returns>
        public double getValue(String competenceId)
        {
            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Key.id == competenceId)
                    return (entry.Value);
            }
            return (-1.0);
        }


        /// <summary>
        /// Method for setting an initial competence state according to "Remarks on the Simplified Update Rule - Cord Hockemeyer"
        /// </summary>
        /// 
        /// <param name="cst"> Spezifies the competence structure for which the competence state is created. </param>
        private void setInitialCompetenceState(CompetenceStructure cst)
        {
            Dictionary<string, double> ups = new Dictionary<string, double>();
            Dictionary<string, double> downs = new Dictionary<string, double>();

            //initialize upsets/downsets
            foreach (Competence com in cst.competences)
            {
                ups[com.id] = 0.0;
                downs[com.id] = 0.0;
            }

            //calculate upsets/downsets
            foreach (Competence com1 in cst.competences)
            {
                foreach (Competence com2 in cst.competences)
                {
                    if (com1.isIndirectPrerequesiteOf(com2))
                    {
                        ups[com1.id] = ups[com1.id] + 1.0;
                        downs[com2.id] = downs[com2.id] + 1.0;
                    }
                }
            }

            foreach (Competence com in cst.competences)
            {
                pairs[com] = (cst.competences.Count + ups[com.id] - downs[com.id] + 1.0) / (2.0 * cst.competences.Count + 2.0);
            }

        }

        /// <summary>
        /// Diagnostic method for displaying competence state
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Competence State:");
            //CompetenceAssessmentHandler.Instance.loggingCA("=================");
            String str = "";
            foreach (var pair in pairs)
            {
                str += "(" + pair.Key.id + ":" + Math.Round(pair.Value, 2) + ")";
                //CompetenceAssessmentHandler.Instance.loggingCA("Key: " + pair.Key.id + " Value: " + Math.Round(pair.Value,2));
            }
            CompetenceAssessmentHandler.Instance.loggingCA(str);

        }

        /// <summary>
        /// Methd for printing out only the mastered competences
        /// </summary>
        public void printMasteredCompetences()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Competences mastered:");
            //CompetenceAssessmentHandler.Instance.loggingCA("=================");
            String str = "";
            foreach (var pair in this.getMasteredCompetences())
            {
                str += "(" + pair.id + ":" + Math.Round(this.getValue(pair.id), 2) + ")";
                //CompetenceAssessmentHandler.Instance.loggingCA("Key: " + pair.Key.id + " Value: " + Math.Round(pair.Value,2));
            }
            CompetenceAssessmentHandler.Instance.loggingCA(str);
        }

        /// <summary>
        /// Method for setting a competence probability by competence name
        /// </summary>
        /// 
        /// <param name="str"> String- specifying for which competence the probability is set. </param>
        /// <param name="v"> Probability value </param>
        private void setCompetenceValue(string str, double v)
        {
            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Key.id == str)
                    pairs[entry.Key] = v; ;
            }

        }

        /// <summary>
        /// Method for setting a competence probability by competence 
        /// </summary>
        /// 
        /// <param name="str"> Specifying for which competence the probability is set. </param>
        /// <param name="v"> Probability value </param>
        public void setCompetenceValue(Competence com, double v)
        {
            pairs[com] = v; ;
        }

        /// <summary>
        /// Returns all mastered Competences.
        /// </summary>
        /// 
        /// <returns> List of all Competences which are assumed to be mastered. </returns>
        public List<Competence> getMasteredCompetences()
        {
            List<Competence> mastered = new List<Competence>();

            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Value >= transitionProbability)
                    mastered.Add(entry.Key);
            }

            return mastered;
        }

        /// <summary>
        /// Returns all mastered Competences as string-list.
        /// </summary>
        /// 
        /// <returns> List of all Competences as strings which are assumed to be mastered. </returns>
        public List<String> getMasteredCompetencesString()
        {
            List<Competence> mastered = getMasteredCompetences();
            List<String> masteredString = new List<string>();
            foreach (Competence com in mastered)
                masteredString.Add(com.id);

            return masteredString;
        }

        #endregion Methods

    }

    /// <summary>
    /// Enum defining the three information - levels of competence evidences 
    /// </summary>
    public enum EvidencePower
    {
        Low,
        Medium,
        High
    }
    
    /// <summary>
    /// Class for storing the possible update properties/powers within the asset
    /// </summary>
    internal class UpdateLevelStorage
    {
        #region Fields

        internal Dictionary<EvidencePower, ULevel> up = new Dictionary<EvidencePower, ULevel>();
        internal Dictionary<EvidencePower, ULevel> down = new Dictionary<EvidencePower, ULevel>();

        #endregion Fields
        #region Constructors

        internal UpdateLevelStorage(DomainModel dm)
        {
            foreach(UpdateLevel ul in dm.updateLevels.updateLevelList)
            {
                ULevel newLevel = new ULevel();
                newLevel.maxonelevel = ul.maxonelevel.Equals("true") ? true : false;
                newLevel.minonecompetence = ul.minonecompetence.Equals("true") ? true : false;
                newLevel.xi = Double.Parse(ul.xi);
                EvidencePower power = (ul.power.Equals("low")) ? EvidencePower.Low : (ul.power.Equals("medium")) ? EvidencePower.Medium : EvidencePower.High;
                if (ul.direction.Equals("up"))
                    up.Add(power, newLevel);
                else if (ul.direction.Equals("down"))
                    down.Add(power, newLevel);
            }
        }

        #endregion Constructors
        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Class describing the properties/power of the evidence
    /// </summary>
    internal class ULevel
    {
        #region Fields
        public double xi;
        public Boolean minonecompetence;
        public Boolean maxonelevel;
        #endregion Fields
    }

    /// <summary>
    /// Stores the mapping between in-game activities and related update procedure
    /// </summary>
    internal class ActivityMapping
    {
        #region Fields

        /// <summary>
        /// Stores activities as keys and Dictionary (Competences + Array(ULevel+up/down)) as Values 
        /// </summary>
        internal Dictionary<String, Dictionary<String, String[]>> mapping = new Dictionary<string, Dictionary<String, String[]>>();

        #endregion Fields
        #region Constructors

        internal ActivityMapping(DomainModel dm)
        {
            if(dm.relations.activities != null && dm.relations.activities.activities != null)
            {
                foreach (ActivitiesRelation ac in dm.relations.activities.activities)
                {
                    Dictionary<String, String[]> newActivityMap = new Dictionary<string, string[]>();
                    foreach (CompetenceActivity cac in ac.competences)
                        newActivityMap.Add(cac.id, new string[] { cac.power, cac.direction });
                    mapping.Add(ac.id, newActivityMap);
                }
            }
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// This Methods updates the competence based on an observed activity
        /// </summary>
        /// <param name="activity"> string representing the observed activity </param>
        internal void updateCompetenceAccordingToActivity(String activity)
        {
            //searching for the activity in the mapping
            Dictionary<String, String[]> competencesToUpdate;
            if (!mapping.ContainsKey(activity))
            {
                CompetenceAssessmentHandler.Instance.loggingCA("The received activity " + activity + " is unknown.");
                return;
            }

            competencesToUpdate = mapping[activity];
            UpdateLevelStorage uls =  CompetenceAssessmentHandler.Instance.updateLevelStorage;

            List<String> competences = new List<string>();
            List<Boolean> evidences = new List<bool>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            foreach(String competence in competencesToUpdate.Keys)
            {
                competences.Add(competence);
                String[] ULevelDirection = competencesToUpdate[competence];
                switch (ULevelDirection[0])
                {
                    case "low":  evidencePowers.Add(EvidencePower.Low); break; 
                    case "medium":  evidencePowers.Add(EvidencePower.Medium); break; 
                    case "high":  evidencePowers.Add(EvidencePower.High); break; 
                    default: throw new Exception("UpdateLevel unknown!");
                }
                switch (ULevelDirection[1])
                {
                    case "up": evidences.Add(true); break;
                    case "down": evidences.Add(false); break;
                    default: throw new Exception("Updatedirection unknown!");
                }
            }

            CompetenceAssessmentHandler.Instance.loggingCA("Performing update based on activity '"+activity+"'.");
            CompetenceAssessmentHandler.Instance.getCAA().updateCompetenceState(competences, evidences, evidencePowers);

        }

        #endregion Methods
    }

    /// <summary>
    /// Stores the mapping between game situations and related update procedure
    /// </summary>
    internal class GameSituationMapping
    {
        #region Fields

        /// <summary>
        /// Stores game situation as keys and Dictionary (Competences + ULevel) as Values 
        /// </summary>
        internal Dictionary<String, Dictionary<String, String>> mappingUp = new Dictionary<string, Dictionary<String, String>>();
        internal Dictionary<String, Dictionary<String, String>> mappingDown = new Dictionary<string, Dictionary<String, String>>();

        #endregion Fields
        #region Constructors

        internal GameSituationMapping(DomainModel dm)
        {
            if(dm.relations.situations != null && dm.relations.situations.situations != null)
            {
                foreach (SituationRelation sr in dm.relations.situations.situations)
                {
                    Dictionary<String, String> newSituationMapUp = new Dictionary<string, string>();
                    Dictionary<String, String> newSituationMapDown = new Dictionary<string, string>();
                    foreach (CompetenceSituation cs in sr.competences)
                    {
                        newSituationMapUp.Add(cs.id, cs.up);
                        newSituationMapDown.Add(cs.id, cs.down);
                    }
                    mappingUp.Add(sr.id, newSituationMapUp);
                    mappingDown.Add(sr.id, newSituationMapDown);
                }
            }
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// This Methods updates the competence based on a gamesituation and information about success/failure
        /// </summary>
        /// <param name="gamesituationId"> string representing the played game situation </param>
        /// <param name="success"> string giving information about the player's success during the game situation </param>
        internal void updateCompetenceAccordingToGamesituation(String gamesituationId, Boolean success)
        {
            //searching for the activity in the mapping
            Dictionary<String, String> competencesToUpdate;
            Dictionary<String, Dictionary<String, String>> mapping = success ? mappingUp : mappingDown;
            if (!mapping.ContainsKey(gamesituationId))
            {
                CompetenceAssessmentHandler.Instance.loggingCA("The received game situation "+gamesituationId+" is unknown.");
                return;
            }

            competencesToUpdate = mapping[gamesituationId];
            UpdateLevelStorage uls = CompetenceAssessmentHandler.Instance.updateLevelStorage;

            List<String> competences = new List<string>();
            List<Boolean> evidences = new List<bool>();
            List<EvidencePower> evidencePowers = new List<EvidencePower>();
            foreach (String competence in competencesToUpdate.Keys)
            {
                competences.Add(competence);
                String ULevel = competencesToUpdate[competence];
                switch (ULevel)
                {
                    case "low": evidencePowers.Add(EvidencePower.Low); break;
                    case "medium": evidencePowers.Add(EvidencePower.Medium); break;
                    case "high": evidencePowers.Add(EvidencePower.High); break;
                    default: throw new Exception("UpdateLevel unknown!");
                }
                evidences.Add(success);
            }

            CompetenceAssessmentHandler.Instance.loggingCA("Performing update based on game situation.");
            CompetenceAssessmentHandler.Instance.getCAA().updateCompetenceState(competences, evidences, evidencePowers);
        }

        #endregion Methods
    }
}
