WDT Assignment 2  - MCBA Web Application
s3804158 - Ruvinda Ranaweera
s3724928 - Luke Di Guglielmo

Reasoning behind Records
Changed the BillPay, Transaction and Login classes as Records, since these data does not need to be modified frequently. 
The data in these Records only need to be read-only and protects against accidental changes in the code.
The Balance of the Account changes frequently, so Account was left as a class.
Adding these classes as Records has a performance benefit at runtime.

References
https://www.w3schools.com/bootstrap4/bootstrap_forms_inputs.asp
https://stackoverflow.com/questions/9054609/how-to-select-a-single-column-with-entity-framework
https://www.learnrazorpages.com/razor-pages/forms/select-lists
https://khalidabuhakmeh.com/generate-outputs-with-razor-engine-in-dotnet-core
https://stackoverflow.com/questions/38762368/embedded-resource-in-net-core-libraries
https://www.yogihosting.com/aspnet-core-identity-roles/#create-role
Sorry forgot to add these reference links:
1)https://www.youtube.com/watch?v=C4O8vqg295o
2)https://stackoverflow.com/questions/44305186/the-smtp-server-has-unexpectedly-disconnected-in-mailkit
3)https://jasonwatmore.com/post/2020/07/15/aspnet-core-3-send-emails-via-smtp-with-mailkit|
4)https://ffimnsr.medium.com/sending-email-using-mailkit-in-asp-net-core-web-api-71b946380442
And the codes provided in lectures and tutorials and linked on canvas.