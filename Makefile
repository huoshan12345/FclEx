render-local:
	ANSIBLE_HASH_BEHAVIOUR=merge \
	ANSIBLE_DISPLAY_OK_HOSTS=false \
	ANSIBLE_DISPLAY_SKIPPED_HOSTS=false \
	ansible-playbook \
		--vault-id huoshan@~/.ansible_vault_password_huoshan \
		--connection=local \
		--inventory 127.0.0.1, \
		-e phase=Local \
		-e @ansible/vars/main.json \
		-e @ansible/vars/main_vault.json \
		ansible/playbooks/render_config.yml

render-github:
	ANSIBLE_HASH_BEHAVIOUR=merge \
	ANSIBLE_DISPLAY_OK_HOSTS=false \
	ANSIBLE_DISPLAY_SKIPPED_HOSTS=false \
	ansible-playbook \
		--vault-id huoshan@~/.ansible_vault_password_huoshan \
		--connection=local \
		--inventory 127.0.0.1, \
		-e phase=Github \
		-e @ansible/vars/main.json \
		-e @ansible/vars/main_vault.json \
		ansible/playbooks/render_config.yml