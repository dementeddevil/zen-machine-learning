/*namespace Zen.Aero.MachineLearning.Neural.FeedForward
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Reflection;

	/// <summary>
	/// Implementation of an artificial neural network which supports
	/// automatic wire up of perceptrons and actuators by using reflection
	/// Derived classes add perceptron properties and actuator methods.
	/// These methods and properties are marked with NeuralSynapse custom
	/// attribute.
	/// </summary>
	[Serializable]
	public class WiredNeuralNetwork : NeuralNetwork
	{
		#region Private Members
		private object hostObject = null;
		private ArrayList validPerceptrons;
		private ArrayList validActuators;
		private int extraInputs = 0;
		private float[] inputMatrix;
		private bool initialised = false;
		#endregion

		#region Public Events
		public event EventHandler PreDrive;
		#endregion

		#region Public Constructors
		public WiredNeuralNetwork()
		{
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		public object HostObject
		{
			get
			{
				if (hostObject == null)
				{
					return this;
				}
				return hostObject; 
			}
			set { hostObject = value; }
		}

		public virtual int ExtraInputCount
		{
			get
			{
				return extraInputs;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException ("value", value, "value cannot be less than zero.");
				}
				if (extraInputs != value)
				{
					if (!Initialising && extraInputs > 0)
					{
						Layers.RemoveAt (0);
					}
					extraInputs = value;
					if (!Initialising && extraInputs > 0)
					{
						base.InputCount = validPerceptrons.Count + extraInputs;
						inputMatrix = new float [base.InputCount];
						Layers.Insert (0, new Layer (base.InputCount));
					}
				}
			}
		}
		#endregion

		#region Public Methods
		public void Drive ()
		{
			if (!initialised)
			{
				if (!Initialising)
				{
					((ISupportInitialize)this).BeginInit ();
				}
				((ISupportInitialize)this).EndInit ();
			}

			// Allow base class to ready itself
			OnPreDrive (EventArgs.Empty);

			// Apply property inputs to neural network
			for (int index = 0; index < validPerceptrons.Count; ++index)
			{
				PropertyInfo prop = (PropertyInfo)validPerceptrons [index];
				inputMatrix [index] = (float)prop.GetValue (hostObject, null);
			}

			// Drive the neural network and interpret outputs
			float[] outputs = GetOutput (inputMatrix);
			FilterDriveOutput (ref outputs);
			ProcessDriveOutput (outputs);
		}
		#endregion

		#region Protected Methods
		protected override void OnBeginInit ()
		{
			// Build brain I/O list
			validPerceptrons = new ArrayList ();
			validActuators = new ArrayList ();
			Layers.Clear ();

			// Notify base class now we have stablised.
			base.OnBeginInit ();
		}
		protected override void OnEndInit ()
		{
			// Enumerate neuron layers
			NeuralLayerAttribute[] attribs = (NeuralLayerAttribute[])
				HostObject.GetType ().GetCustomAttributes (typeof (NeuralLayerAttribute), true);
			if (attribs != null)
			{
				int[] neuronCounts = new int [attribs.Length];
				foreach (NeuralLayerAttribute attrib in attribs)
				{
					if (attrib.Layer >= 0 && attrib.Layer < attribs.Length)
					{
						neuronCounts [attrib.Layer] = attrib.NeuronCount;
					}
				}

				// Create layer objects containing neuron count
				for (int index = 0; index < neuronCounts.Length; ++index)
				{
					if (neuronCounts [index] > 0)
					{
						Layers.Add (neuronCounts [index]);
					}
				}
			}

			// Enumerate properties
			// NOTE: Only float properties are considered.
			//	Readable properties can be perceptors
			//	Writable properties can be actuators although these
			//	actuator types are not included in the winner-takes-all
			//	implementation.
			PropertyInfo[] properties = HostObject.GetType ().GetProperties ();
			foreach (PropertyInfo prop in properties)
			{
				NeuralSynapseAttribute[] attrs = (NeuralSynapseAttribute[])
					prop.GetCustomAttributes (
					typeof (NeuralSynapseAttribute), true);
				if (attrs.Length > 0)
				{
					for (int index = 0; index < attrs.Length; ++index)
					{
						if (prop.CanRead &&
							attrs [index].NodeType == SynapseType.Perceptron)
						{
							validPerceptrons.Add (prop);
						}
						if (prop.CanWrite &&
							attrs [index].NodeType == SynapseType.Actuator)
						{
							validActuators.Add (prop);
						}
					}
				}
			}

			// Enumerate methods
			// NOTE: Methods can only be actuators and can optionally
			//	take a single argument of type float
			MethodInfo[] methods = HostObject.GetType ().GetMethods ();
			foreach (MethodInfo method in methods)
			{
				NeuralSynapseAttribute[] attrs = (NeuralSynapseAttribute[])
					method.GetCustomAttributes (
					typeof (NeuralSynapseAttribute), true);
				if (attrs.Length > 0)
				{
					// Take the first attribute only
					if (attrs [0].NodeType == SynapseType.Actuator)
					{
						validActuators.Add (method);
					}
				}
			}

			// Insert new entry and exit layers
			InputCount = validPerceptrons.Count + extraInputs;
			Layers.Insert (0, new Layer (InputCount));
			Layers.Add (validActuators.Count);

			// Setup internal neural network
			inputMatrix = new float [base.InputCount];

			// Notify base class now we have finished.
			base.OnEndInit ();
			initialised = true;
		}

		/// <summary>
		/// PreDrive is called by the network simulus prior to actually
		/// running the input pattern through the network.
		/// Override in derived classes to setup the network perceptrons.
		/// </summary>
		protected virtual void OnPreDrive (EventArgs e)
		{
			if (PreDrive != null)
			{
				PreDrive (this, e);
			}
		}

		/// <summary>
		/// FilterDriveOutput is called immediately after the network
		/// has processed the perceptron inputs.
		/// Override this method to adjust the network outputs prior
		/// to selection of a winner.
		/// </summary>
		/// <param name="outputs"></param>
		protected virtual void FilterDriveOutput (ref float[] outputs)
		{
		}

		/// <summary>
		/// PreDriveOutput provides the final opportunity to adjust the
		/// network output index.
		/// </summary>
		/// <param name="bestIndex"></param>
		/// <returns></returns>
		protected virtual int PreDriveOutput (int bestIndex)
		{
			return bestIndex;
		}

		/// <summary>
		/// OnDriveOutput method is called prior to firing the winning
		/// neural actuator method and fires all interested delegates
		/// When overriding this method remember to call the base class
		/// to ensure events are raised.
		/// </summary>
		/// <param name="bestIndex"></param>
		protected virtual void OnDriveOutput (int bestIndex)
		{
			// TODO: Fire some kind of event
		}

		/// <summary>
		/// ProcessDriveOutput method handles making the call through to
		/// a given neural actuator method. It also takes care of setting
		/// the property based neural actuators before making the actual
		/// call.
		/// </summary>
		/// <param name="outputs"></param>
		protected virtual void ProcessDriveOutput (float[] outputs)
		{
			// By default this method implements a winner takes all
			//	approach to driving the outputs
			// Consider only methods for the winner but setup any
			//	properties with a suitable value
			float bestOutput = outputs [0];
			int bestIndex = 0;
			for (int index = 0; index < outputs.Length; ++index)
			{
				if (outputs [index] >= bestOutput &&
					validActuators [index] is MethodInfo)
				{
					bestOutput = outputs [index];
					bestIndex = index;
				}
				if (validActuators [index] is PropertyInfo)
				{
					PropertyInfo prop = (PropertyInfo)validActuators [index];
					prop.SetValue (hostObject, outputs [index], null);
				}
			}

			// Notify of choice found (can be modified)
			int altIndex = PreDriveOutput (bestIndex);
			if (altIndex != bestIndex &&
				altIndex > 0 && altIndex < validActuators.Count)
			{
				bestIndex = altIndex;
			}

			// Notify of choice found (cannot be modified)
			OnDriveOutput (bestIndex);

			// Find actuation method and prepare to call through
			MethodInfo method = (MethodInfo)validActuators [bestIndex];
			ParameterInfo[] args = method.GetParameters ();
			if (args.Length == 0)
			{
				method.Invoke (hostObject, null);
			}
			else if (args.Length == 1 && args [0].ParameterType == typeof(float))
			{
				object[] methodArgs = 
					{
						bestOutput
					};
				method.Invoke (hostObject, methodArgs);
			}
			else
			{
				throw new InvalidOperationException ("Unable to invoke actuator method (" +
					GetType ().FullName + "." + method.Name + ")");
			}
		}
		#endregion
	}

	#region Implementation of NeuralSynapse attribute
	public enum SynapseType
	{
		Perceptron,
		Actuator,
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class NeuralSynapseAttribute : Attribute
	{
		private SynapseType nodeType;

		public NeuralSynapseAttribute (SynapseType nodeType)
		{
			this.nodeType = nodeType;
		}

		public SynapseType NodeType
		{
			get { return this.nodeType; }
		}
	}
	#endregion

	#region Implementation of NeuralLayer attribute
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NeuralLayerAttribute : Attribute
	{
		private int neuronCount;
		private int layer;

		public NeuralLayerAttribute (int neuronCount)
		{
			this.neuronCount = neuronCount;
			this.layer = 0;
		}

		public NeuralLayerAttribute (int neuronCount, int layer)
		{
			this.neuronCount = neuronCount;
			this.layer = layer;
		}

		public int NeuronCount
		{
			get { return this.neuronCount; }
		}

		public int Layer
		{
			get { return this.layer; }
		}
	}
	#endregion
}
*/