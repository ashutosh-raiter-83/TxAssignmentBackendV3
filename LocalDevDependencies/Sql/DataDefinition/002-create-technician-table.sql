CREATE TABLE sortbot.technician (
    technician_id varchar NOT NULL,
	username varchar NOT NULL,
	password varchar NOT NULL,
	CONSTRAINT technician_pk PRIMARY KEY (technician_id),
	CONSTRAINT technician_unique UNIQUE (username)
);
