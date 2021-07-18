package crc642c62280bb41d5e97;


public class GattServerCallback
	extends android.bluetooth.BluetoothGattServerCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onConnectionStateChange:(Landroid/bluetooth/BluetoothDevice;II)V:GetOnConnectionStateChange_Landroid_bluetooth_BluetoothDevice_IIHandler\n" +
			"n_onCharacteristicReadRequest:(Landroid/bluetooth/BluetoothDevice;IILandroid/bluetooth/BluetoothGattCharacteristic;)V:GetOnCharacteristicReadRequest_Landroid_bluetooth_BluetoothDevice_IILandroid_bluetooth_BluetoothGattCharacteristic_Handler\n" +
			"n_onCharacteristicWriteRequest:(Landroid/bluetooth/BluetoothDevice;ILandroid/bluetooth/BluetoothGattCharacteristic;ZZI[B)V:GetOnCharacteristicWriteRequest_Landroid_bluetooth_BluetoothDevice_ILandroid_bluetooth_BluetoothGattCharacteristic_ZZIarrayBHandler\n" +
			"n_onDescriptorWriteRequest:(Landroid/bluetooth/BluetoothDevice;ILandroid/bluetooth/BluetoothGattDescriptor;ZZI[B)V:GetOnDescriptorWriteRequest_Landroid_bluetooth_BluetoothDevice_ILandroid_bluetooth_BluetoothGattDescriptor_ZZIarrayBHandler\n" +
			"n_onDescriptorReadRequest:(Landroid/bluetooth/BluetoothDevice;IILandroid/bluetooth/BluetoothGattDescriptor;)V:GetOnDescriptorReadRequest_Landroid_bluetooth_BluetoothDevice_IILandroid_bluetooth_BluetoothGattDescriptor_Handler\n" +
			"";
		mono.android.Runtime.register ("LightScout.Droid.GattServerCallback, LightScout.Android", GattServerCallback.class, __md_methods);
	}


	public GattServerCallback ()
	{
		super ();
		if (getClass () == GattServerCallback.class)
			mono.android.TypeManager.Activate ("LightScout.Droid.GattServerCallback, LightScout.Android", "", this, new java.lang.Object[] {  });
	}


	public void onConnectionStateChange (android.bluetooth.BluetoothDevice p0, int p1, int p2)
	{
		n_onConnectionStateChange (p0, p1, p2);
	}

	private native void n_onConnectionStateChange (android.bluetooth.BluetoothDevice p0, int p1, int p2);


	public void onCharacteristicReadRequest (android.bluetooth.BluetoothDevice p0, int p1, int p2, android.bluetooth.BluetoothGattCharacteristic p3)
	{
		n_onCharacteristicReadRequest (p0, p1, p2, p3);
	}

	private native void n_onCharacteristicReadRequest (android.bluetooth.BluetoothDevice p0, int p1, int p2, android.bluetooth.BluetoothGattCharacteristic p3);


	public void onCharacteristicWriteRequest (android.bluetooth.BluetoothDevice p0, int p1, android.bluetooth.BluetoothGattCharacteristic p2, boolean p3, boolean p4, int p5, byte[] p6)
	{
		n_onCharacteristicWriteRequest (p0, p1, p2, p3, p4, p5, p6);
	}

	private native void n_onCharacteristicWriteRequest (android.bluetooth.BluetoothDevice p0, int p1, android.bluetooth.BluetoothGattCharacteristic p2, boolean p3, boolean p4, int p5, byte[] p6);


	public void onDescriptorWriteRequest (android.bluetooth.BluetoothDevice p0, int p1, android.bluetooth.BluetoothGattDescriptor p2, boolean p3, boolean p4, int p5, byte[] p6)
	{
		n_onDescriptorWriteRequest (p0, p1, p2, p3, p4, p5, p6);
	}

	private native void n_onDescriptorWriteRequest (android.bluetooth.BluetoothDevice p0, int p1, android.bluetooth.BluetoothGattDescriptor p2, boolean p3, boolean p4, int p5, byte[] p6);


	public void onDescriptorReadRequest (android.bluetooth.BluetoothDevice p0, int p1, int p2, android.bluetooth.BluetoothGattDescriptor p3)
	{
		n_onDescriptorReadRequest (p0, p1, p2, p3);
	}

	private native void n_onDescriptorReadRequest (android.bluetooth.BluetoothDevice p0, int p1, int p2, android.bluetooth.BluetoothGattDescriptor p3);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
