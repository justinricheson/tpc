# TPC #

TPC stands for Trivially Parallel Computer. It's an attempt at a generic distributed processor. It uses vector clocks and a modified Ricart-Agrawala algorithm for mutual exclusion and leader election. Simply implement IProcessor to do any sort of trivially parallel processing. It's still pretty unstable and only has semi-working fault recovery using timeouts.