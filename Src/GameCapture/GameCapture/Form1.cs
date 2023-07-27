using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using EO.WebBrowser;
using NetFwTypeLib;
namespace GameCapture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        private static int width = Screen.PrimaryScreen.Bounds.Width;
        private static int height = Screen.PrimaryScreen.Bounds.Height;
        private static bool Getstate;
        public static Form2 form2 = new Form2();
        public static int[] wd = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static int[] wu = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static bool[] ws = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        static void valchanged(int n, bool val)
        {
            if (val)
            {
                if (wd[n] <= 1)
                {
                    wd[n] = wd[n] + 1;
                }
                wu[n] = 0;
            }
            else
            {
                if (wu[n] <= 1)
                {
                    wu[n] = wu[n] + 1;
                }
                wd[n] = 0;
            }
            ws[n] = val;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            this.pictureBox1.Dock = DockStyle.Fill;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(pictureBox1.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            Navigate("https://michaelandrefraniatte.web.app/ppia/");
            try
            {
                form2.Show();
            }
            catch { }
            Task.Run(() => Start());
        }
        public void Start()
        {
            for (; ; )
            {
                valchanged(0, GetAsyncKeyState(Keys.NumPad0));
                valchanged(1, GetAsyncKeyState(Keys.Decimal));
                if (wd[1] == 1 & !Getstate)
                {
                    try
                    {
                        this.webView1.EvalScript(@"setTimeout(function(){ 
                        try {
                                $('#starting').click();
                        }
                        catch {}
                        }, 300);");
                    }
                    catch { }
                    Getstate = true;
                }
                else
                {
                    if (wd[0] == 1 & Getstate)
                    {
                        try
                        {
                            this.webView1.EvalScript(@"setTimeout(function(){ 
                            try {
                                $('#stoping').click();
                            }
                            catch {}
                            }, 300);");
                        }
                        catch { }
                        Getstate = false;
                    }
                }
                Thread.Sleep(70);
            }
        }
        private void webView1_RequestPermissions(object sender, RequestPermissionEventArgs e)
        {
            e.Allow();
        }
        private void webView1_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            Task.Run(() => LoadPage());
        }
        private void LoadPage()
        {
            string stringinject;
            stringinject = @"
    <style>
         
	.container, a {
		display:flex; 
		flex-direction: column; 
		justify-content: center; 
		align-items: center;
	}

	.container-buttons {
		text-align: center;
	}

    .selection {
        float: left;
    }

    </style>
".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(" + stringinject + @" ).appendTo('head');";
            this.webView1.EvalScript(stringinject);
            stringinject = @"
    
    <div class='selection'>
        <select id='contentselection' onchange='changeContent(this)'>
            <option>Webcam</option>
            <option>Desktop</option>
        </select>
    </div>
    <div class='container'>
        <div class='container-buttons'>
            <button onclick='startVisualizer()' id='starting'>Start</button>
            <button onclick='stopVisualizer()' id='stoping'>Stop</button>
            <button onclick='playVisualizer()' id='playing'>Play</button>
        </div>
        <video id='video' autoplay='true' width='600' height='auto'></video>
        <canvas id='visualizer' width='600' height='50'></canvas>
    </div>

    <script>

var webcam = true;

function changeContent(newcontent) {
    if (newcontent.options[newcontent.selectedIndex].text == 'Webcam') {
        webcam = true;
    }
    if (newcontent.options[newcontent.selectedIndex].text == 'Desktop') {
        webcam = false;
    }
}

function AudioVisualizer(video, canvas) {
    this.video = video;
    this.canvas = canvas;
    this.mediaRecorder = null;
    this.mediaChunks = [];
    this.recordStartTimestamp = null;
    this.recordTimerId = null;
    this.visualDrawTimer = null;
}

var audioTrack = '';
var videoTrack = '';

AudioVisualizer.prototype = {

    mediaProps : {
        audio: true,
        video: true
    },

    start() {
        if (webcam) {
                navigator.mediaDevices.getUserMedia ({
                audio: true,
                video: true
            })
            .then( stream => {
                this.mediaSource = stream;
                this.video.srcObject = stream;
                this.visualize();
                this.startRecording();
            })
            .catch();
        }
        else {
            navigator.mediaDevices.getUserMedia ({
            audio: true
        })
        .then(audioStream => {
             [audioTrack] = audioStream.getAudioTracks();
             navigator.mediaDevices.getDisplayMedia({
             video: true
        })
        .then(displayStream => {
             [videoTrack] = displayStream.getVideoTracks();
             videoTrack.contentHint = 'detail';
             stream = new MediaStream([videoTrack, audioTrack]);
             this.mediaSource = stream;
             this.video.srcObject = stream;
             this.visualize();
             this.startRecording();
        })})
        .catch();
        }
    },

    startRecording() {
        this.mediaRecorder = new MediaRecorder(this.mediaSource);
        this.mediaChunks = [];
        this.video.muted = true;
        this.mediaRecorder.addEventListener('dataavailable', event => {
            this.mediaChunks.push(event.data);
        });
        this.mediaRecorder.onstop = this.recordStopped.bind(this);
        this.mediaRecorder.start();
    },

    stopRecording() {
        if (this.mediaRecorder) {
            this.mediaRecorder.stop();
        }

        if (this.mediaSource && this.mediaSource.getTracks) {
            for (const track of this.mediaSource.getTracks()) {
                track.stop();
            }
            this.stopVisualizer();
            this.mediaSource = null;
        }
    },

    recordStopped(event) {
        const mediaBlob = new Blob(this.mediaChunks, { 'type' : 'video/mp4' });
        this.mediaSource = mediaBlob;
	    var blobUrl = URL.createObjectURL(this.mediaSource);
        $('a').remove();
        var link = document.createElement('a');
	    link.href = blobUrl;
	    link.download = 'output.mp4';
	    link.innerHTML = 'Click here to download the file';
	    document.body.appendChild(link);
    },

    async visualize() {
        var stream = this.mediaSource;
        if (!stream)
            return;

        this.stopVisualizer();

        var canvas = this.canvas;
        var WIDTH = canvas.width;
        var HEIGHT = canvas.height;

        var ctx = canvas.getContext('2d');

        var audioContext = new (window.AudioContext || window.webkitAudioContext)();
        var analyser = audioContext.createAnalyser();
        var dataArray = new Uint8Array(analyser.frequencyBinCount);
        
        if (stream instanceof Blob) {
            const arrayBuffer = await new Response(stream).arrayBuffer();
            const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
            source = audioContext.createBufferSource();
            source.buffer = audioBuffer;
            source.connect(analyser);
            source.start(0);
        }
        else {
            var source = audioContext.createMediaStreamSource(stream);
            source.connect(analyser);
        }

        analyser.fftSize = 1024;
        var bufferLength = analyser.fftSize;
        var dataArray = new Uint8Array(bufferLength);

        ctx.clearRect(0, 0, WIDTH, HEIGHT);

        var draw = () => {

            this.visualDrawTimer = requestAnimationFrame(draw);

            analyser.getByteTimeDomainData(dataArray);

            ctx.fillStyle = 'wheat';
            ctx.fillRect(0, 0, WIDTH, HEIGHT);

            ctx.lineWidth = 2;
            ctx.strokeStyle = 'black';

            ctx.beginPath();

            var sliceWidth = WIDTH * 1.0 / bufferLength;
            var x = 0;

            for(var i = 0; i < bufferLength; i++) {

                var v = dataArray[i] / 128.0;
                var y = v * HEIGHT/2;

                if(i === 0) {
                    ctx.moveTo(x, y);
                }
                else {
                    ctx.lineTo(x, y);
                }

                x += sliceWidth;
            }

            ctx.lineTo(WIDTH, HEIGHT/2);
            ctx.stroke();
        };
        draw();
    },

    stopVisualizer() {
        if (this.visualDrawTimer) {
            window.cancelAnimationFrame(this.visualDrawTimer);
            this.visualDrawTimer = null;
        }
    },

    playRecording() {
        this.video.muted = false;
        this.video.srcObject = null;
        this.video.src = URL.createObjectURL(this.mediaSource);
        this.visualize();
    }
};

var proto = '';

function startVisualizer() {
    const video = document.getElementById('video');
    const canvas = document.getElementById('visualizer');
    proto = new AudioVisualizer(video, canvas);
    proto.start();
}

function stopVisualizer() {
    proto.stopRecording();
}

function playVisualizer() {
    proto.playRecording();
}

    </script>
".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('body').append(" + stringinject + @");});";
            this.webView1.EvalScript(stringinject);
        }
        private void Navigate(string address)
        {
            if (String.IsNullOrEmpty(address))
                return;
            if (address.Equals("about:blank"))
                return;
            if (!address.StartsWith("http://") & !address.StartsWith("https://"))
                address = "https://" + address;
            try
            {
                webView1.Url = address;
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webView1.Dispose();
        }
    }
}