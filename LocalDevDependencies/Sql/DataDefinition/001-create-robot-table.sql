CREATE TABLE sortbot.robot (
    robot_id varchar NOT NULL,
	username varchar NOT NULL,
	password varchar NOT NULL,
	last_log_in_at timestamptz NULL,
	CONSTRAINT robot_pk PRIMARY KEY (robot_id),
	CONSTRAINT robot_unique UNIQUE (username)
);

CREATE INDEX robot_last_log_in_at_idx ON sortbot.robot (last_log_in_at);
