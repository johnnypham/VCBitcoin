# VCBitcoinhttps://github.com/johnnypham/VCBitcoin

VCBitcoin is a Bitcoin blockchain parser, block explorer and JSON-RPC library for Bitcoin Core.

Blockchain parser: Deserializes the entire blockchain and stores the data in a PostgreSQL database. Stale blocks are removed after all blocks are processed. Indexes are created on the most relevant fields.

Block explorer: A fully featured block explorer that can retrieve balances of addresses, display all inputs and outputs of a transaction, and more. Custom SQL queries can also be executed on the database to extract other useful information and perform statistical analyses, e.g. chart the difficulty of mining over time, determine the percentage of segwit transactions over a certain period of time, etc.

JSON-RPC client: Many full nodes run on dedicated hardware not suitable for development, e.g. older systems, single-board computers, remote web servers. VCBitcoin provides a simple interface to send RPC queries to a remote or local Bitcoin full node.



Example SQL queries



To get the difficulty of mining:

SELECT TargetDifficulty

    FROM Block

    ORDER BY Timestamp ASC


To calculate the percentage of transactions that are Segwit in a certain time period:

select count(1) from transaction where blockhash in (select blockhash from block where timestamp > '2018-11-04 02:30 EDT'::timestamptz


To get the balance of an address:

First, iterate through all the outputs:

SELECT SUM(Value) 
FROM OUTPUT 
WHERE address = 'mwx4oxbWUGXXSSrcipRPAkRSoYi7FFHHia'

Then, find any outputs which have been used as inputs in another transaction:

SELECT SUM(Value) 
FROM Output o
WHERE address = 'mwx4oxbWUGXXSSrcipRPAkRSoYi7FFHHia' AND 
EXISTS (SELECT TransactionID, Vout FROM Input i WHERE
	   o.TransactionID = i.TransactionID AND 
	   o.Index = i.Vout)

Subtract the second sum from the first sum to get the final balance.





