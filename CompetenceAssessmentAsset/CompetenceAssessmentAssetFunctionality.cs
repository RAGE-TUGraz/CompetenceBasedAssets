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
using System.Threading;

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
        /// Instance of the tracker asset
        /// </summary>
        private TrackerAsset tracker = null;

        /// <summary>
        /// Instance of the game storage asset
        /// </summary>
        internal GameStorageClientAsset gameStorage = null;

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
        private CompetenceAssessmentHandler() {}

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

            //reset gameStorage
            gameStorage = null;

            createCompetenceState(cstr);
            this.updateLevelStorage = new UpdateLevelStorage(dm);
            this.gameSituationMapping = new GameSituationMapping(dm);
            this.activityMapping = new ActivityMapping(dm);

            //reset gameStorage part2
            getGameStorageAsset();

            loadCompetenceStateFromGameStorage();
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

            //before the update, load the competence state, if needed
            if (gameStorage == null)
                loadCompetenceStateFromGameStorage();

            cstr.updateCompetenceState(csta, compList, evidenceList,  evidencePowers);

        }

        /// <summary>
        /// Method for sending the current probabilities for possessing a competence to the tracker
        /// </summary>
        internal void storeCompetenceStateToGameStorage()
        {
            CompetenceAssessmentAssetSettings caas = getCAA().getSettings();
            String model = "CompetenceAssessmentAsset_" + getCAA().getSettings().PlayerId + "_" + competenceStructure.domainModelId;


            CompetenceState cs =  getCompetenceState();
            Dictionary<Competence,double> competenceValues =  cs.getCurrentValues();

            //storing the data
            GameStorageClientAsset storage = getGameStorageAsset();
            foreach (Competence competence in competenceValues.Keys)
                storage[model][competence.id].Value =  competenceValues[competence];

            //storing the updated data
            storage.SaveData(model, StorageLocations.Local, SerializingFormat.Json);
            loggingCA("Competencestate stored locally.");

            //send data to the tracker
            sendCompetenceValuesToTracker();
        }

        /// <summary>
        /// Method for loading the competence state.
        /// </summary>
        internal void loadCompetenceStateFromGameStorage()
        {
            GameStorageClientAsset storage = getGameStorageAsset();

            CompetenceAssessmentAssetSettings caas = getCAA().getSettings();
            String model = "CompetenceAssessmentAsset_" + getCAA().getSettings().PlayerId + "_" + competenceStructure.domainModelId;


            storage.LoadData(model, StorageLocations.Local, SerializingFormat.Json);

            //storing data in data structure
            CompetenceState cs = getCompetenceState();
            Dictionary<Competence, double> competenceValues = cs.getCurrentValues();


            foreach (Node node in storage[model].Children)
                cs.setCompetenceValue(competenceStructure.getCompetenceById(node.Name), (double)node.Value);


            loggingCA("Competence values restored from local file.");
            

            
        }

        /// <summary>
        /// Method returning the client game storage asset
        /// </summary>
        /// <returns></returns>
        internal GameStorageClientAsset getGameStorageAsset()
        {
            if(gameStorage == null)
            {
                gameStorage = new GameStorageClientAsset();
                gameStorage.Bridge = AssetManager.Instance.Bridge;
                
                StorageLocations storageLocation = StorageLocations.Local;
                

                if (competenceStructure == null)
                    getCAA().getCompetenceState();

                //try to load model, if possible -> load competence state, else create model and store model + competence state
                String model = "CompetenceAssessmentAsset_"+ getCAA().getSettings().PlayerId +"_"+ competenceStructure.domainModelId;

                gameStorage.AddModel(model);
                Boolean isStructureRestored = gameStorage.LoadStructure(model, storageLocation);
                if (isStructureRestored)
                {
                    loggingCA("Structure was restored from local file.");
                    loadCompetenceStateFromGameStorage();
                }
                else
                {
                    loggingCA("Structure could not be restored from local file - creating new one.");
                    CompetenceState cs = this.getCompetenceState();
                    foreach (Competence comp in cs.getCurrentValues().Keys)
                        gameStorage[model].AddChild(comp.id, storageLocation).Value = cs.getValue(comp);

                    gameStorage.SaveStructure(model, storageLocation);
                    gameStorage.SaveData(model, storageLocation, SerializingFormat.Json);
                }
                

            }
            return gameStorage;
        }

        /// <summary>
        /// Method for sending the competence state to the tracker
        /// </summary>
        internal void sendCompetenceValuesToTracker()
        {
            //get the tracker
            if(tracker == null)
            {
                if (AssetManager.Instance.findAssetsByClass("TrackerAsset").Count >= 1)
                {
                    tracker = (TrackerAsset)AssetManager.Instance.findAssetsByClass("TrackerAsset")[0];
                    loggingCA("Found tracker for tracking competence values!");
                }
                else
                {
                    /*
                    loggingCA("No tracker implemented - creating new one");
                    tracker = TrackerAsset.Instance;
                    TrackerAssetSettings tas = new TrackerAssetSettings();
                    tas.BasePath = "/api/";
                    tas.Host = "192.168.222.167";
                    tas.TrackingCode = "5784a7c1e8c85f6e00fab465gdj3utijicin3ik9"; 
                    tas.Secure = false;
                    tas.Port = 3000;
                    tas.StorageType = TrackerAsset.StorageTypes.net;
                    tas.TraceFormat = TrackerAsset.TraceFormats.xapi;
                    tracker.Settings = tas;
                    //*/
                    
                    
                    //*no tracking
                    loggingCA("No tracker implemented - competence state is not send to the server");
                    return;
                    //*/
                    
                    /*
                    //local tracking
                    loggingCA("No tracker implemented - competence state is not send to the server - tracks are stored local!");
                    TrackerAsset ta = TrackerAsset.Instance;
                    TrackerAssetSettings tas = new TrackerAssetSettings();
                    tas.StorageType = TrackerAsset.StorageTypes.local;
                    tas.TraceFormat = TrackerAsset.TraceFormats.json;
                    ta.Settings = tas;
                    //*/
                }
            }

            if (tracker.CheckHealth())
            {
                loggingCA(tracker.Health);
                CompetenceAssessmentAssetSettings caas = getCAA().getSettings();
                if (tracker.Login(caas.TrackerName, caas.TrackerPassword))
                {
                    loggingCA("logged in - tracker");
                }
                else
                {
                    loggingCA("Maybe you forgot to store name/password for the tracker to the Competence Assessment Asset Settings.");
                }
            }

            if (tracker.Connected)
            {
                tracker.Start();
                Dictionary<Competence, Double> cs = getCompetenceState().getCurrentValues();
                //Double mean = 0;
                foreach(Competence competence in cs.Keys)
                {
                    tracker.setVar(competence.id, cs[competence].ToString());
                    //mean += cs[competence];
                }
                //tracker.Completable.Initialized("CompetenceAssessmentAsset");
                //tracker.Completable.Progressed("CompetenceAssessmentAsset",(float) mean/cs.Count);
                tracker.Completable.Completed("CompetenceAssessmentAsset");
                //tracker.Accesible.Accessed("CompetenceAssessmentAsset");
                
                //TEST MULTITHREADING
                new Thread(() =>
                {
                    //next line: thread is killed after all foreground threads are dead
                    Thread.CurrentThread.IsBackground = true;
                    //code goes here:
                    tracker.Flush();
                }).Start();
            }
            else
            {
                loggingCA("Not connected to tracker.");
            }
        }

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

        /// <summary>
        /// Method for resetting the current competence state to the starting competence state
        /// </summary>
        public void resetCompetenceState()
        {
            //registerNewPlayer(getDMA().getDomainModel());
            String model = "CompetenceAssessmentAsset_" + getCAA().getSettings().PlayerId + "_" + competenceStructure.domainModelId;
            //getCAA().getCompetenceState();
            CompetenceState cs = new CompetenceState(new CompetenceStructure(getDMA().getDomainModel()));
            foreach (Competence competence in cs.getCurrentValues().Keys)
                gameStorage[model][competence.id].Value = cs.getCurrentValues()[competence];

            //storing the updated data
            gameStorage.SaveData(model, StorageLocations.Local, SerializingFormat.Json);
            loadCompetenceStateFromGameStorage();
            loggingCA("Competencestate reset.");
            //registerNewPlayer(getDMA().getDomainModel());

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

        #endregion TestMethods

    }

    /// <summary>
    /// Class representing the Competence-Tree of the Domainmodel.
    /// </summary>
    public class CompetenceStructure
    {
        #region Fields 

        /// <summary>
        /// Domainmodel-ID, consistent of concatenation of all competences in lexicographic order
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
        public CompetenceStructure(DomainModel dm)
        {

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

            List<String> competenceNames = new List<string>();
            foreach (Competence comp in this.competences)
                competenceNames.Add(comp.id);
            competenceNames.Sort();

            domainModelId = "";
            foreach (String id in competenceNames)
                domainModelId +="&"+id;
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
        public Competence getCompetenceById(String id)
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

            CompetenceAssessmentHandler.Instance.storeCompetenceStateToGameStorage();

        }

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
            String str0 = "In CS:  ";
            foreach (Competence c in cs.getMasteredCompetences())
                str0 += c.id + ",";
            CompetenceAssessmentHandler.Instance.loggingCA(str0);

            String str = com.id+" Possible competences to shift minonecompetence:  ";
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
                            if ((!possibleCompetencesToShiftMaxOneLevel.Contains(comp)) && (com.isIndirectPrerequesiteOf(comp) || comp.isIndirectPrerequesiteOf(com)))
                                possibleCompetencesToShiftMaxOneLevel.Add(comp);
                }
                else
                {
                    foreach (Competence competence in possibleCompetencesToShiftMinOneLevel)
                        foreach (Competence comp in competence.getPrerequisiteWithAllSuccessorsNotInCompetenceStateButThis(cs))
                            if ((!possibleCompetencesToShiftMaxOneLevel.Contains(comp)) && (com.isIndirectPrerequesiteOf(comp) || comp.isIndirectPrerequesiteOf(com)))
                                possibleCompetencesToShiftMaxOneLevel.Add(comp);
                }

                /*
                String str2 = "Possible competences to shift maxonelevel:  ";
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


                //downgrading->make sure to lose not more than one competence level
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

            //logging
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
        /// Method determining, if all prerequisites to one competence are met
        /// </summary>
        /// <param name="cs"> Competence for which this is determined </param>
        /// <returns> True, if all prerequisites are met, false otherwise</returns>
        public Boolean allPrerequisitesMet(Dictionary<string,double> cs)
        {
            Boolean allPrerequisitesMet = true;
            foreach (Competence com in this.prerequisites)
            {
                if (cs[com.id] < CompetenceAssessmentHandler.Instance.transitionProbability)
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
            if(dm.updateLevels != null && dm.updateLevels.updateLevelList != null)
            {
                foreach (UpdateLevel ul in dm.updateLevels.updateLevelList)
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
            else
            {
                CompetenceAssessmentHandler.Instance.loggingCA("No update-levels specified for the competence assessment!");
                throw new Exception("No update-levels specified for the competence assessment!");
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
