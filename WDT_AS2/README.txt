Reasoning behind Records
Changed the Customer, Transaction and Login classes as Records, since these data does not need to be modified frequently. 
The data in these Records only need to be read-only and protects against accidental changes in the code.
The Balance of the Account changes frequently, so Account was left as a class.
Adding these classes as Records has a performance benefit at runtime.