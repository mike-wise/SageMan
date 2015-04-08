# SageMan
Sage (Azure Recommender API) Command Line Runner
   Mike Wise - 8 April 2015

This is a program that allows one to excersize the Azure Recommender API via a command line. 
It is currently fairly rudiementary, but it provided with source code so that it can be easily added ti.
Since it retains some state across calls using the .NET Application Settings feature, you don't have
to input the user name and account key for every call. 

A typical useage might be to:
 1. Set the user name and account key into the settings.
 2. Create a model.
 3. Load the model with catalog data.
 4. Load the model with usage data.
 5. Build the model (this can take some time depending on the size of the data).
 6. Obtain some recommendations from the model.
 5. Delete the model.
 
 
 To Do:
  - Add an example catalog file and usage file to the mix.
  - Add commands that should execute that use the above data.
 
 
