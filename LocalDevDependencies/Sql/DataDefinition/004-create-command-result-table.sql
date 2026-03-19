CREATE TABLE sortbot.received_command_result (
	command_id varchar NOT NULL,
    robot_id varchar NOT NULL,
	command_result json NOT NULL,
	received_at timestamptz DEFAULT timezone('utc'::text, now()) NOT NULL,
	CONSTRAINT received_command_result_pk PRIMARY KEY (command_id, robot_id)
);

CREATE INDEX received_command_result_received_at_idx ON sortbot.received_command_result (robot_id, received_at);

ALTER TABLE sortbot.received_command_result
ADD CONSTRAINT received_command_result_sent_command_robot_fk
FOREIGN KEY (command_id, robot_id)
REFERENCES sortbot.sent_command(command_id, robot_id)
ON DELETE RESTRICT
ON UPDATE RESTRICT;
