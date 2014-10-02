using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RunFrames : MonoBehaviour
{

    #region structs

    [System.Serializable]
    public struct FramesFolder              // Struct to store the information about a specific take
    {
        public string folderName;           // Directory (Resources/<FOLDER>) that the frames are located - must have "/" at the end
        public string framePrefix;          // The prefix of the frame names ("save." by default from RGDB Toolkit)
        public int startIndex;              // The starting frame number of your sequence
        public int numberOfFrames;          // How long your sequence is
        public int frameRate;               // The frame rate that your sequence was captured at (RGB camera)
        public AudioClip audioTrack;        // The accompanying audio clip (frame numbers should match)
    }

    #endregion

    #region inspector
    
    private bool _playMesh = true;
    public bool playMesh                    // Toggle to display/play hologram
    {
        get { return _playMesh; }
        set
        {
            gameObject.renderer.enabled = value;
            _playMesh = value;
        }
    }

    public bool loopSequence = false;       // Loop the sequence back to 0 once it is finished
    public bool continuousPlay = false;     // Starts playing the next take in the sequence once current one is finished

    public List<FramesFolder> sequences;    // The list of takes (displayed in the inspector)

    //Audio section
    public AudioSource videoSound;          // Audio Source on the GameObject should be dragged hre
    private int videoSoundStartIndex;       // Global for finding where to start the audio clip
    public int soundSampleFrequency = 48000;// Frequency of the sound clip in Hz
    public int sampleSyncFrequency = 48000; // How often to sync the image to the sound (in Hz - eg. 48000 is every second)
    public int sampleSyncFrames = 30;       // How many frames exist per sync period (30fps = 30 at 48000 sync freq)
    public int frameCompensation = 1;       // How much to increment the frame number beyond the calculated sync point

    #endregion

    #region take variables

    private int numberOfFrames;              // The number of frames in the current take
    private int frameRate = 30;              // The frame rate of the current take

    //Frame storage
    private List<Texture2D[]> frameArrays = new List<Texture2D[]>();    // Store for the frames to be loaded into memory at Start
    private Texture2D[] currFrameArray;     // Pointer to the current take frames
    public int sequenceIndex = 0;           // The index of which take to use from the take list (sequences)
	private int frameNum = 0;               // The current frame number

    #endregion

    #region runtime variables

    private float timeAtStart;              // Unity Time.Time at the start of playing the sequence (for frame calculation)
    private int nextSampleSync = 1;         // Counter to check when to next sync audio/video (based on sampleSyncFrequency)

    #endregion

    // Loads all frames into RAM from the file structure
	private void LoadImages(int sequenceToRead, string folderName, string framePrefix, int startIndex, int numberOfFrames)
	{
		if (framePrefix.Equals ("")) 
		{
			framePrefix = @"save.";
		}

        //Add new array to the list of frame arrays
        frameArrays.Add(new Texture2D[numberOfFrames+2]);
	     
	    for (int i=startIndex-1; i < numberOfFrames+startIndex; i++)
	    {
			// Prepends the correct amount of zeroes to the current frame number
	    	string index = "";
	    	float logIdx = Mathf.Log10(i+1);
	    	
	    	if (logIdx < 1.0)
	    		index += "0000";
	    	else if (logIdx < 2.0)
	    		index += "000";
			else if (logIdx < 3.0)
	    		index += "00";
			else if (logIdx < 4.0)
	    		index += "0";
			else if (logIdx < 5.0)
				index += "";
			else Debug.Log("Too many frames in animation!");
	    	
	    	index += (i+1);

			// Create a Texture2D from the specified image file
			Texture2D frame = Resources.Load<Texture2D>(folderName + framePrefix + index);
	     
			// Add this frame to the sequences list in RAM
            frameArrays[sequenceToRead][i + 1 - startIndex] = frame;
		}
	}

    // Loads the take, including the texture PNGs and the global variables for frame rate, etc.
    public void LoadSequence(int index)
    {
        sequenceIndex = index;

        // Calculate the time sample that the audio clip should start at, based on the start frame number
        videoSoundStartIndex = (int)((float)(sequences[sequenceIndex].startIndex - 1) / sequences[sequenceIndex].frameRate * soundSampleFrequency);
        frameRate = sequences[sequenceIndex].frameRate;
        numberOfFrames = sequences[sequenceIndex].numberOfFrames;
        if (videoSound != null && sequences[sequenceIndex].audioTrack != null)
        {
            videoSound.clip = sequences[sequenceIndex].audioTrack;
        }
        currFrameArray = frameArrays[sequenceIndex];
        frameNum = 0;
    }
	
	void Start () {
		//QualitySettings.vSyncCount = 0;
		//Application.targetFrameRate = frameRate;

        if (sequences.Count > 0)
        {
            // Load all the takes into RAM
            for (int i = 0; i < sequences.Count; ++i)
            {
                LoadImages(i, sequences[i].folderName, sequences[i].framePrefix, sequences[i].startIndex, sequences[i].numberOfFrames);
            }

            //Load the global variables for the sequence
            LoadSequence(0);
        }
	}
	
	void Update () {
        if (_playMesh || Input.GetKeyDown(KeyCode.A))
        {
            // Reset start time if at frame 0
            if (frameNum == 0)
            {
                timeAtStart = Time.time;
            }

            // If restarting the loop, reset the audio clip
            if (frameNum == 0 && videoSound != null)
            {
                videoSound.Stop();
                videoSound.timeSamples = videoSoundStartIndex;
                videoSound.Play();
                nextSampleSync = 1;
            }

            // Calculate which frame to display (to decouple from the game's frame rate)
            float timeDiff = Time.time - timeAtStart;
            int frameToShow = (int)(timeDiff * frameRate);
            frameNum = frameToShow;

            frameNum = Mathf.Min(frameNum, numberOfFrames - 1);     // Limit frame number to the last frame

            // Display the texture on the mesh
            Texture2D tex = currFrameArray[frameNum];
            renderer.material.SetTexture("_MainTex", tex);

            //frameNum = (frameNum + 1) % numberOfFrames;           // To perform the frame looping
            frameNum = frameNum + 1; // Increment frame

            // Move onto next take in sequence if at the end of this take, or triggered by keypress
            if (frameNum >= numberOfFrames || Input.GetKeyDown(KeyCode.A))
            {
                frameNum = numberOfFrames - 1;                      // Load final frame
                tex = currFrameArray[frameNum];
                renderer.material.SetTexture("_MainTex", tex);

                //LoadSequence((sequenceIndex + 1) % sequences.Count);  // Wrap around after last take in sequence
                sequenceIndex += 1;
                if (sequenceIndex < sequences.Count || loopSequence)    // If not on last take of sequence, cue next take
                {
                    LoadSequence(sequenceIndex % sequences.Count);
                    frameNum = 0;
                }

                if (!continuousPlay)
                {
                    _playMesh = false;                              // Freeze
                }
                return;
            }

            // Check if visual has fallen behind audio, catch up by changing the frame number (sync video to audio)
            if (frameNum != 0 && videoSound != null && videoSound.timeSamples > videoSoundStartIndex + nextSampleSync * sampleSyncFrequency)
            {

                if (frameNum + 1 < nextSampleSync * sampleSyncFrames)
                {
                    //Debug.Log("Fallen behind: " + videoSound.timeSamples);
                    frameNum = (nextSampleSync * sampleSyncFrames) + frameCompensation;
                    timeAtStart = Time.time - ((float)frameNum / frameRate);
                    if (frameNum > numberOfFrames)
                    {
                        frameNum = 0;
                    }
                }
                nextSampleSync += 1;
            }
        }
	}
}
