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
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-22
*/

namespace DomainModelAssetNameSpace
{
    using AssetPackage;
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// An asset settings.
    /// 
    /// BaseSettings contains the (de-)serialization methods.
    /// </summary>
    public class DomainModelAssetSettings : BaseSettings
    {
        #region Fields
        /// <summary>
        /// If true the domain model is loaded from a local xml file.
        /// </summary>
        bool localSource = true;

        /// <summary>
        /// If true the domain model is loaded via a web request.
        /// </summary>
        bool webSource = false;

        /// <summary>
        /// Defines where to load the domain model from. Either a fileId when using a local xml file or a url, when loading from a website.
        /// </summary>
        string source = "dm.xml";
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DomainModelAsset.AssetSettings class.
        /// </summary>
        public DomainModelAssetSettings()
            : base()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// If true the domain model is loaded from a local xml file. Exactly one field from LocalSource/WebSource need to be true.
        /// </summary>
        [XmlElement()]
        public bool LocalSource
        {
            get { return (localSource); }
            set { localSource = value; webSource = !value; }
        }

        /// <summary>
        /// If true the domain model is loaded via a web request. Exactly one field from LocalSource/WebSource need to be true.
        /// </summary>
        [XmlElement()]
        public bool WebSource
        {
            get { return (webSource); }
            set { localSource = !value; webSource = value; }
        }

        /// <summary>
        /// Defines where to load the domain model from. Either a fileId when using a local xml file or a url, when loading from a website.
        /// </summary>
        [XmlElement()]
        public String Source
        {
            get { return source; }
            set { source = value; }
        }
        
        #endregion Properties
    }
}
