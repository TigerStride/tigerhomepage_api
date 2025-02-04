/* Set up the Tigerstride database */
DROP DATABASE IF EXISTS tiger_contact;
CREATE DATABASE tiger_contact;
use tiger_contact;

/* Create a MySQL user with read and write permissions */
CREATE USER 'contact_user'@'%' IDENTIFIED BY 'secure_password';
GRANT SELECT, INSERT, UPDATE, DELETE ON tiger_contact.customer_message TO 'contact_user'@'%';
FLUSH PRIVILEGES;

CREATE table customer_message 
(
	Id INT auto_increment PRIMARY KEY,
	customerName varchar(100) NOT NULL,
	customerEmail varchar(100) NOT Null,
	messageText TEXT NULL,
	createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

/* unit test */
insert into customer_message (customerName, customerEmail, messageText)
values ('Test User', 'test@none.com', 'This is a test message.');

select *
from customer_message;