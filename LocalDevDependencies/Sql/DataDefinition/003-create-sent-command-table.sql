CREATE TABLE sortbot.sent_command (
	command_id varchar NOT NULL,
    robot_id varchar NOT NULL,
	command json NOT NULL,
	sent_at timestamptz NOT NULL,
	CONSTRAINT sent_command_pk PRIMARY KEY (command_id, robot_id)
);

CREATE INDEX sent_command_sent_at_idx ON sortbot.sent_command (robot_id, sent_at);

ALTER TABLE
	sortbot.sent_command
ADD CONSTRAINT sent_command_robot_fk
FOREIGN KEY (robot_id)
REFERENCES sortbot.robot(robot_id)
ON DELETE RESTRICT
ON UPDATE RESTRICT;
