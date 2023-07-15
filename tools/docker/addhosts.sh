#!/bin/bash

# Function to add an entry if it doesn't exist
function add_host_entry {
    ip_address=$1
    hostname=$2
    if ! grep -q "$ip_address $hostname" /etc/hosts; then
        echo "$ip_address $hostname" | sudo tee -a /etc/hosts
    fi
}

# Function to remove entries based on domain
function remove_host_entry {
    undesired_domain=$1
    sudo sed -i "/$undesired_domain/d" /etc/hosts
}

# Usage
add_host_entry "192.168.1.1" "example.com"
add_host_entry "192.168.1.2" "example.org"
remove_host_entry "example.org"
