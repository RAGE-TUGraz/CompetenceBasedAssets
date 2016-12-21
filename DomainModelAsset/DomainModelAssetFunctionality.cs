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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DomainModelAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling (read DM from Web/File, store DM to File) the Domainmodel (DM)
    /// </summary>
    internal class DomainModelHandler
    {
        #region Fields

        /// <summary>
        /// Instance of the DomainModelAsset
        /// </summary>
        private DomainModelAsset domainModelAsset = null;

        /// <summary>
        /// Instance of the class DomainModelHandler - Singelton pattern
        /// </summary>
        static readonly DomainModelHandler instance = new DomainModelHandler();

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private Boolean doLogging = true;

        /// <summary>
        /// Run-time Asset storage of domain model.
        /// </summary>
        private DomainModel domainModel = null;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private DomainModelHandler-ctor for Singelton-pattern 
        /// </summary>
        private DomainModelHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the DomainModelHandler - Singelton pattern
        /// </summary>
        public static DomainModelHandler Instance
        {
            get
            {
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

        /// <summary>
        /// DomainModel Property
        /// </summary>
        internal DomainModel DomainModel
        {
            get
            {
                return getDomainModel();
            }
        }

        #endregion Properties
        #region Methods 

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
        /// Method returning domain model either from the run-tima asset storage if available or from specified (default) source(File/Web).
        /// </summary>
        /// 
        /// <returns> The domein model. </returns>
        private DomainModel getDomainModel()
        {
            if (domainModel != null)
                return domainModel;
            DomainModel dm = loadDefaultDomainModel();
            domainModel = dm;
            return dm;
        }

        /// <summary>
        /// Method for setting the domain model
        /// </summary>
        /// <param name="dm"> The new doamin model</param>
        internal void setDomainModel(DomainModel dm)
        {
            domainModel = dm;
        }
        
        /// <summary>
        /// Method for storing a domain model.
        /// </summary>
        /// <param name="dm"> Domain model to store. </param>
        internal void storeDomainModel(DomainModel dm)
        {
            domainModel = dm;
        }
        
        /// <summary>
        /// Method loading domain model - location specified by settings.
        /// </summary>
        /// <returns>Domain Model for the player.</returns>
        internal DomainModel loadDefaultDomainModel()
        {
            loggingDM("Loading default Domain model.");
            DomainModelAssetSettings dmas = (DomainModelAssetSettings) getDMA().Settings;

            if (dmas.LocalSource)
            {
                IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
                if (ids != null )
                {
                    if (!ids.Exists(dmas.Source))
                    {
                        loggingDM("File "+ dmas.Source + " not found for loading Domain model.", Severity.Error);
                        throw new Exception("EXCEPTION: File "+ dmas.Source + " not found for loading Domain model.") ;
                    }

                    loggingDM("Loading DomainModel from File.");
                    return (this.getDMFromXmlString(ids.Load(dmas.Source)));
                }
                else
                {
                    loggingDM("IDataStorage bridge absent for requested local loading method of the Domain model.", Severity.Error);
                    throw new Exception("EXCEPTION: IDataStorage bridge absent for requested local loading method of the Domain model.");
                }
            }
            else
            {
                IWebServiceRequest iwr = (IWebServiceRequest)AssetManager.Instance.Bridge;
                if (iwr != null)
                {
                    loggingDM("Loading web DomainModel.");
                    Uri uri = new Uri(dmas.Source);
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    //headers.Add("user", playerId);
                    //string body = dmas.Source;
                    WebServiceResponse wsr = new WebServiceResponse();
                    //currentPlayerId = playerId;

                    RequestSetttings rs = new RequestSetttings();
                    rs.method = "GET";
                    rs.uri = uri;
                    rs.requestHeaders = headers;
                    //rs.body = body;

                    RequestResponse rr = new RequestResponse();

                    iwr.WebServiceRequest(rs, out rr);
                    return (this.getDMFromXmlString(rr.body));
                }
                else
                {
                    loggingDM("IWebServiceRequest bridge absent for requested web loading method of the Domain model.", Severity.Error);
                    throw new Exception("EXCEPTION: IWebServiceRequest bridge absent for requested web loading method of the Domain model.");
                }
            }

        }

        /// <summary>
        /// Method for deserialization of a XML-String to the coressponding Domainmodel.
        /// </summary>
        /// 
        /// <param name="str"> String containing the XML-Domainmodel for deserialization. </param>
        ///
        /// <returns>
        /// DomainModel-type coressponding to the parameter "str" after deserialization.
        /// </returns>
        internal DomainModel getDMFromXmlString(String str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DomainModel));
            using (TextReader reader = new StringReader(str))
            {
                DomainModel result = (DomainModel)serializer.Deserialize(reader);
                return (result);
            }
        }

        /// <summary>
        /// Method for storing a DomainModel as XML in a File.
        /// </summary>
        /// 
        /// <param name="dm"> Domainmodel to store. </param>
        /// <param name="fileId"> String containing the file identification. </param>
        internal void writeDMToFile(DomainModel dm, String fileId)
        {
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                loggingDM("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                loggingDM("No IDataStorage - Bridge implemented!",Severity.Warning);

        }
        
        #endregion Methods
        #region TestMethods

        /// <summary>
        /// Diagnostic logging method.
        /// </summary>
        /// 
        /// <param name="msg"> String to be logged.  </param>
        /// <param name="severity"> Severity of the logging-message, optional. </param>
        internal void loggingDM(String msg, Severity severity = Severity.Information)
        {
            if (DoLogging)
            {
                if(domainModelAsset==null)
                    domainModelAsset = (DomainModelAsset) AssetManager.Instance.findAssetByClass("DomainModelAsset");
                domainModelAsset.Log(severity, "[DMA]: " +msg);
            }
        }
        
        #endregion TestMethods

    }
    
    /// <summary>
    /// Implementation of the WebServiceResponse-Interface for handling web requests.
    /// </summary>
    public class WebServiceResponse 
    {
        /// <summary>
        /// Describes behaviour in case the web request failed.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msg"></param>
        public void Error(string url, string msg)
        {
            DomainModelHandler.Instance.loggingDM("Web Request for retriving Domain model from "+url+" failed! " + msg, Severity.Error);
            throw new Exception("EXCEPTION: Web Request for retriving Domain model from " + url + " failed! " + msg);
        }

        /// <summary>
        /// Describes behaviour in case the web requests succeeds
        /// </summary>
        /// <param name="url"></param>
        /// <param name="code"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public void Success(string url, int code, Dictionary<string, string> headers, string body)
        {
            DomainModelHandler.Instance.loggingDM("WebClient request successful!");
            DomainModelHandler.Instance.storeDomainModel(DomainModelHandler.Instance.getDMFromXmlString(body));
        }
    }

    #region Serializing 
    /// <summary>
    /// Class containing all Domainmodel data.
    /// </summary>
    [XmlRoot("domainmodel")]
    public class DomainModel
    {
        #region Properties
        
        [XmlElement("elements")]
        public Elements elements { get; set; }

        [XmlElement("relations")]
        public Relations relations { get; set; }
        
        [XmlElement("updatelevels")]
        public UpdateLevels updateLevels { get; set; }


        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("Printing out DM:");
            elements.print();
            relations.print();
            if(updateLevels != null)
                updateLevels.print();
        }

        #region Methods

    public String toXmlString()
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(DomainModel));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    String xml = stringWriter.ToString();

                    return (xml);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        #endregion Methods
    }

    public class UpdateLevels
    {
        #region Fields

        [XmlElement("level")]
        public List<UpdateLevel> updateLevelList { get; set; }
       
        #endregion Fields
        #region Methods

        public void print()
        {
             DomainModelHandler.Instance.loggingDM("-Printing out updateLevels:");
             foreach(UpdateLevel ul in updateLevelList)
                ul.print();
        }

        #endregion Methods
    }

    public class UpdateLevel
    {

        #region Fields

        [XmlAttribute("direction")]
        public string direction { get; set; }

        [XmlAttribute("power")]
        public string power { get; set; }

        [XmlAttribute("xi")]
        public string xi { get; set; }

        [XmlAttribute("minonecompetence")]
        public string minonecompetence { get; set; }

        [XmlAttribute("maxonelevel")]
        public string maxonelevel { get; set; }

        #endregion Fields


        #region Methods

        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-----");
            DomainModelHandler.Instance.loggingDM("--direction:" +direction);
            DomainModelHandler.Instance.loggingDM("--power:" + power);
            DomainModelHandler.Instance.loggingDM("--xi:" + xi);
            DomainModelHandler.Instance.loggingDM("--minonecompetence:" + minonecompetence);
            DomainModelHandler.Instance.loggingDM("--maxonelevel:" + maxonelevel);

        }

        #endregion Methods

    }
    
    public class Elements
    {
        #region Properties
        
        [XmlElement("competences")]
        public CompetenceList competences { get; set; }
        [XmlElement("situations")]
        public SituationsList situations { get; set; }
        [XmlElement("activities")]
        public ActivityList activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out elements:");
            if (competences != null)
                competences.print();
            if (situations != null)
                situations.print();
            if (activities != null)
                activities.print();
        }
    }

    public class ActivityList
    {
        #region Properties

        [XmlElement("activity")]
        public List<Activity> activityList { get; set; }
        
        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Activities:");
            foreach (Activity ac in activityList)
            {
                ac.print();
            }
        }
    }

    public class Activity
    {
        #region Properties
    
        [XmlAttribute("id")]
        public string id { get; set; }

        #endregion Properties
        #region Constructors

        public Activity() { }

        public Activity(string id)
        {
            this.id = id;
        }

        #endregion Constructors
        #region Methods
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id:" + id);
        }
        #endregion Methods
    }
    
    public class SituationsList
    {
        #region Properties

        [XmlElement("situation")]
        public List<Situation> situationList { get; set; }
        
        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Situations:");
            foreach (Situation si in situationList)
            {
                si.print();
            }
        }
    }

    public class Situation
    {
        #region Properties

        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("title")]
        public string title { get; set; }
        [XmlAttribute("uri")]
        public string uri { get; set; }

        #endregion Properties
        #region Constructor

        /// <summary>
        /// default c-tor
        /// </summary>
        public Situation() { }

        /// <summary>
        /// c-tor with id
        /// </summary>
        /// <param name="id"> Situation identifier </param>
        public Situation(String id)
        {
            this.id = id;
        }

        #endregion Constructor

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---");
            DomainModelHandler.Instance.loggingDM("---id:" + id);
            DomainModelHandler.Instance.loggingDM("---title:" + title);
            DomainModelHandler.Instance.loggingDM("---uri:" + uri);
        }
    }

    public class CompetenceList
    {
        #region Properties

        [XmlElement("competence")]
        public List<CompetenceDesc> competenceList { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Competences:");
            foreach (CompetenceDesc comp in competenceList)
                comp.print();
        }
    }

    public class CompetenceDesc
    {

        #region Properties

        [XmlAttribute("description")]
        public string description { get; set; }
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("title")]
        public string title { get; set; }
        [XmlAttribute("uri")]
        public string uri { get; set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// default c-tor
        /// </summary>
        public CompetenceDesc() { }

        /// <summary>
        /// c-tor with id
        /// </summary>
        /// <param name="id"> competence identifier </param>
        public CompetenceDesc(String id)
        {
            this.id = id;
        }

        #endregion Constructor

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---");
            DomainModelHandler.Instance.loggingDM("---description:" + description);
            DomainModelHandler.Instance.loggingDM("---id:" + id);
            DomainModelHandler.Instance.loggingDM("---title:" + title);
            DomainModelHandler.Instance.loggingDM("---uri:" + uri);
        }
    }

    public class Relations
    {
        #region Properties
    
        [XmlElement("competenceprerequisites")]
        public CompetenceprerequisitesList competenceprerequisites { get; set; }

        [XmlElement("situations")]
        public SituationRelationList situations { get; set; }
        [XmlElement("activities")]
        public ActivityRelationList activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out relations:");
            if (competenceprerequisites != null)
                competenceprerequisites.print();
            if (situations != null)
                situations.print();
            if (activities != null)
                activities.print();
        }
    }

    public class SituationRelationList
    {
        #region Properties

        [XmlElement("situation")]
        public List<SituationRelation> situations { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out situation-Relation:");
            foreach (SituationRelation sr in situations)
            {
                sr.print();
            }
        }

        /*
        public void addSituationRelation(string situationId, string competenceId)
        {
            Boolean found = false;
            foreach(SituationRelation sr in this.situations)
            {
                if (sr.id.Equals(situationId))
                {
                    CompetenceSituation competence = new CompetenceSituation();
                    competence.id = competenceId;
                    competence.up = "medium";
                    competence.down = "medium";
                    sr.competences.Add(competence);
                    found = true;
                    break;
                }
            }
            if (!found)
                situations.Add(new SituationRelation(situationId,new String[] { competenceId }));
        }
        */
    }

    public class SituationRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public List<CompetenceSituation> competences { get; set; }

        #endregion Properties
        #region Constructors

        public SituationRelation() { }

        public SituationRelation(string situationId, string[] competenceId)
        {
            id = situationId;
            competences = new List<CompetenceSituation>();
            foreach(String cid in competenceId)
            {
                CompetenceSituation competence = new CompetenceSituation();
                competence.id = cid;
                competence.up = "medium";
                competence.down = "medium";
                competences.Add(competence);
            }
        }

        #endregion Constructors
        #region Methds
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---------------");
            DomainModelHandler.Instance.loggingDM("---id pr.:" + id);
            foreach(CompetenceSituation cs in this.competences)
                cs.print();
        }

        #endregion Methods
    }

    public class CompetenceSituation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        
        [XmlAttribute("levelup")]
        public String up { get; set; }
        
        [XmlAttribute("leveldown")]
        public String down { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
            DomainModelHandler.Instance.loggingDM("---up comp.:" + up);
            DomainModelHandler.Instance.loggingDM("---down comp.:" + down);
        }
    }

    public class ActivityRelationList
    {
        #region Properties

        [XmlElement("activity")]
        public List<ActivitiesRelation> activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out activities-Relation:");
            foreach (ActivitiesRelation ar in activities)
            {
                ar.print();
            }
        }

        /*
        public void addActivityRelation(String activitytId, String competenceId)
        {
            Boolean found = false;
            foreach(ActivitiesRelation ar in this.activities)
                if(ar.id == activitytId)
                {
                    CompetenceActivity clo = new CompetenceActivity();
                    clo.id = competenceId;
                    ar.competences.Add(clo);
                    found = true;
                    break;
                }
            if (!found)
                this.activities.Add(new ActivitiesRelation(activitytId, competenceId ));
        }
        */
    }

    public class ActivitiesRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public List<CompetenceActivity> competences { get; set; }

        #endregion Properties
        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public ActivitiesRelation() { }

        /// <summary>
        /// c-tor with learningobject id and competence id for a competence which is part of the learning object
        /// </summary>
        /// 
        /// <param name="activitytId">activity id</param>
        /// <param name="competenceId">competence id for a competence which is part of the learning object</param>
        public ActivitiesRelation(String activitytId, CompetenceActivity[] competenceId)
        {
            this.id = activitytId;
            this.competences = new List<CompetenceActivity>();
            foreach(CompetenceActivity cid in competenceId)
                this.competences.Add(cid);
        }

        public ActivitiesRelation(String id, string competenceId)
        {
            this.id = id;
            competences = new List<CompetenceActivity>();
            competences.Add(new CompetenceActivity(competenceId,"medium","up"));
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id lo.:" + id);
            foreach(CompetenceActivity ca in this.competences)
                ca.print();
        }
    }

    public class CompetenceActivity
    {

        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        [XmlAttribute("power")]
        public String power { get; set; }

        [XmlAttribute("direction")]
        public String direction { get; set; }

        #endregion Properties
        #region Constructors

        public CompetenceActivity() { }

        public CompetenceActivity(string id, string power, string direction)
        {
            this.id = id;
            this.power = power;
            this.direction = direction;
        }

        #endregion Constructors
        #region Methods
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
            DomainModelHandler.Instance.loggingDM("---power comp.:" + power);
            DomainModelHandler.Instance.loggingDM("---direction comp.:" + direction);
        }
        #endregion Methods
    }

    public class CompetenceprerequisitesList
    {
        #region Properties

        [XmlElement("competence")]
        public List<CompetenceP> competences { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Competenceprerequisites");
            foreach (CompetenceP cp in competences)
            {
                cp.print();
            }
        }

        /*
        public void addPrerequisiteCompetence(String id, String prerequisiteID)
        {
            Boolean found = false;
            foreach(CompetenceP cp in this.competences)
            {
                if (cp.id.Equals(id))
                {
                    Prereqcompetence precom = new Prereqcompetence();
                    precom.id = prerequisiteID;
                    cp.prereqcompetences.Add(precom);
                    found = true;
                    break;
                }
            }
            if (!found)
                competences.Add(new CompetenceP(id, new String[] { prerequisiteID }));
        }
        */
    }

    public class CompetenceP
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("prereqcompetence")]
        public List<Prereqcompetence> prereqcompetences { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public CompetenceP() { }

        /// <summary>
        /// modified c-tor
        /// </summary>
        /// 
        /// <param name="id"> id of the competence </param>
        /// <param name="prerequisiteID"> id of the prerequisite competence </param>
        public CompetenceP(String id, String[] prerequisiteIDs)
        {
            this.id = id;
            prereqcompetences = new List<Prereqcompetence>();
            foreach (String pid in prerequisiteIDs)
            {
                Prereqcompetence precom = new Prereqcompetence();
                precom.id = pid;
                prereqcompetences.Add(precom);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id: " + id);
            foreach(Prereqcompetence pc in prereqcompetences)
                pc.print();
        }
    }

    public class Prereqcompetence
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id prereq.:" + id);
        }
    }

    #endregion Serializing 
}
