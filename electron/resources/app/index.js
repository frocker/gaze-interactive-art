const { app, BrowserWindow} = require('electron');
const path = require('path');

let mainWindow = undefined;

var ipc = require('electron').ipcMain;

//Client set up
var PORT = 33333;
var HOST = '127.0.0.1';
var dgram = require('dgram');
var server = dgram.createSocket('udp4');
var previousTimestamp = 0;

server.on('listening', function () {
    var address = server.address();
});

server.on('message', function (message, remote) {
    parseMessage(message);
});

function parseMessage(message){
  var messageObj = JSON.parse(message);
  //Drop packets recieved out of order
  if(messageObj.timestamp>previousTimestamp){
    if(mainWindow !== undefined){
      //Send gaze data to open window
      mainWindow.webContents.send('gaze-pos', messageObj);
    }
    previousTimestamp = messageObj.timestamp;
  }
}

server.bind(PORT, HOST);

app.on('ready', () => {
  mainWindow = new BrowserWindow({
    title: 'Gaze Interactive Art',
    backgroundColor: '#000000',
    show: false
  }
  );
  mainWindow.setFullScreen(true);
  mainWindow.loadURL(path.join('file://', __dirname, 'menu.html'));
  mainWindow.webContents.on('did-finish-load', function() {
    mainWindow.show();  
  });
});

ipc.on('open', function (e, painting, design) {
  mainWindow.loadURL(path.join('file://', __dirname, 'index.html?painting='+painting+"&design="+design));
});

ipc.on('menu', function (e) {
  mainWindow.loadURL(path.join('file://', __dirname, 'menu.html'));
});