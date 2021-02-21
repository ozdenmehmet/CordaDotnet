using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CordaApp.CustomClass
{
    public class NodeService
    {
        public bool NodeCreate(string _nodename)
        {
            string fileName = _nodename.ToLower();
            fileName = Regex.Replace(fileName, @"\s+", "");
            try
            {
                //Get previous port number and create new port.
                string locationa = ("ls").Bash();
                int port = Int16.Parse("cd source && cat lastport.txt".Bash());
                string setnewPort = "cd source && : > lastport.txt && echo " + (port + 3) + " >> lastport.txt";
                setnewPort.Bash();

                //Set node credentials and create conf file.
                //HARDCODED
                string nodename = _nodename;
                string location = "New York";
                string country = "US";
                string confCreateCommand = "mkdir out && cd out && touch " + fileName + "_node.conf && : > " + fileName + "_node.conf";
                confCreateCommand.Bash();

                string confDetails = @"
devMode=true
myLegalName=""O=" + nodename + @",L=" + location + @",C=" + country + @"""
p2pAddress=""" + fileName + @":" + port + @"""
rpcSettings {
    address=""0.0.0.0:" + (port + 1) + @"""
    adminAddress=""0.0.0.0:" + (port + 2) + @"""
}
security {
    authService {
        dataSource {
            type=INMEMORY
            users=[
                {
                    password=test
                    permissions=[
                        ALL
                    ]
                    user=user1
                }
            ]
        }
    }
}";
                string confFileWrite = "cd out && tee -a " + fileName + "_node.conf << " + confDetails;
                confFileWrite.Bash();

                //Copying cordapps and corda-bootstrapper
                "cp source/cordapps/* out && cp source/corda-bootstrapper.jar out".Bash();

                //Generate node folder with corda-bootstrapper
                "cd out && java -jar corda-bootstrapper.jar".Bash();

                //Create persistence folder
                string persistenceFolder = "cd out/" + fileName + " && mkdir persistence && cp persistence.mv.db persistence && cp persistence.trace.db persistence";
                persistenceFolder.Bash();

                //Create deployment
                "mkdir deployment".Bash();

                //Create necessary folders
                "cd deployment && mkdir shared && mkdir shared/cordapps && mkdir shared/node-infos".Bash();

                //Copying shared files to deployment
                string sharedFiles = "cp source/cordapps/* deployment/shared/cordapps && cp out/" + fileName + "/additional-node-infos/* source/node-infos && cp source/node-infos/* deployment/shared/node-infos && cp source/network-parameters deployment/shared";
                sharedFiles.Bash();

                //Copy node folder to deployment
                string nodeFolder = "mv out/" + fileName + " deployment/";
                nodeFolder.Bash();

                //Create yaml file for deployment to docker image
                string yamlFileCreate = "cd deployment && touch docker-compose.yaml && : > docker-compose.yaml";
                yamlFileCreate.Bash();
                string yamlDetails = @"
version: '3.5'  
services:
  " + fileName + @": 
    image: corda/corda-zulu-java1.8-4.7
    container_name: " + fileName + @" 
    ports:
      - """ + (port + 1) + @":" + (port + 1) + @"""
    hostname: 127.0.0.1
    volumes:
      - ./" + fileName + @"/node.conf:/etc/corda/node.conf
      - ./" + fileName + @"/certificates:/opt/corda/certificates
      - ./" + fileName + @"/persistence:/opt/corda/persistence
      - ./" + fileName + @"/logs:/opt/corda/logs
      - ./shared/cordapps:/opt/corda/cordapps
      - ./shared/node-infos:/opt/corda/additional-node-infos
      - ./shared/network-parameters:/opt/corda/network-parameters
networks:
  default:
    name: corda-network";


                string yamlFileWrite = "cd deployment && tee -a docker-compose.yaml << " + yamlDetails;
                yamlFileWrite.Bash();

                //Broadcast new node info to exisiting nodes
                string result = "docker ps -a -q".Bash();
                string[] containerArray = result.Split("\n");
                for (int i = 0; i < containerArray.Length; i++)
                {
                    if (containerArray[i] == "") continue;
                    string nodeInfo = "cd deployment/" + fileName + " && docker cp additional-node-infos/* " + containerArray[i] + ":/opt/corda/additional-node-infos";
                    nodeInfo.Bash();
                }



                //Deploy node to Docker
                Console.WriteLine("cd deployment && docker-compose up -d && sleep 30".Bash());

                //Clear folders
                //"rm -rf out && rm -rf deployment".Bash();

                Console.WriteLine("Node created!");
                Console.WriteLine("Name: " + nodename);
                Console.WriteLine("RPC Port: " + (port + 1));

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public bool NodeDelete(string nodeName)
        {
            string _nodeName = nodeName.ToLower();
            _nodeName = Regex.Replace(_nodeName, @"\s+", "");
            try
            {
                string deleteNodeCommand = "docker rm -f " + _nodeName;
                deleteNodeCommand.Bash();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
