Reasoning behind Records
Changed the BillPay, Transaction and Login classes as Records, since these data does not need to be modified frequently. 
The data in these Records only need to be read-only and protects against accidental changes in the code.
The Balance of the Account changes frequently, so Account was left as a class.
Adding these classes as Records has a performance benefit at runtime.

References
https://www.w3schools.com/bootstrap4/bootstrap_forms_inputs.asp
https://stackoverflow.com/questions/9054609/how-to-select-a-single-column-with-entity-framework
https://www.learnrazorpages.com/razor-pages/forms/select-lists
And the codes provided in lectures and tutorials.