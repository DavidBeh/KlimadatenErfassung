# install dotnet to /usr/local/dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh | sudo bash /dev/stdin -InstallDir "/usr/local/dotnet" -Channel LTS
curl -sSL https://aka.ms/getvsdbgsh | sudo /bin/sh /dev/stdin -v latest -l /usr/local/vsdbg
# add env variables
echo 'export DOTNET_ROOT=/usr/local/dotnet' | sudo tee -a /etc/profile > /dev/null
# shellcheck disable=SC2016
echo 'export PATH=$PATH:/usr/local/dotnet' | sudo tee -a /etc/profile > /dev/null
source ~/.bashrc

# create users for debug and production
sudo useradd -m dndebug
echo dndebug:debug | sudo chpasswd
sudo useradd -m dnproduction
echo dnproduction:amogus | sudo chpasswd

