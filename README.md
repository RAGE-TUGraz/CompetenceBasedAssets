# CompetenceBasedAssets

The Competence-based Assets are Asset developed from TU Graz for the RAGE Project (http://rageproject.eu/).


# Domain Model Asset

This asset will be implemented as a client-side component. It is concerned with supplying a domain model (data structure containing competence related data) for a certain player. Therefore, either a server-side component is used, or a local stored domain model is loaded. 

# Competence Assessment Asset

This asset will be implemented as a client-side component. Based on a Domain Model and information from the game it assesses the competence state (= set of possessed competence) of a player.

# Competence Recommendation Asset

This asset will be implemented as client-side component. It decides for a given domain model and a given competence state on how the game proceeds (expressed as next game situations, represented by identification strings). 
