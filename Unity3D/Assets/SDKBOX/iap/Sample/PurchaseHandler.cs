using UnityEngine;
using System.Collections.Generic;
using Sdkbox;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 Google IAB 所有處理
 * 
 * 
 * ***************************************************************
 *                           ChangeLog                  
 * ****************************************************************/
public class PurchaseHandler : MonoBehaviour
{
	private Sdkbox.IAP _iap;

	// Use this for initialization
	void Start()
	{
		_iap = FindObjectOfType<Sdkbox.IAP>();
		if (_iap == null)
		{
			Debug.Log("Failed to find IAP instance");
		}
	}

	public void getProducts()
	{
		if (_iap != null)
		{
			Debug.Log ("About to getProducts, will trigger onProductRequestSuccess event");
			_iap.getProducts ();
		}
	}

	public void Purchase(string item)
	{
		if (_iap != null)
		{
			Debug.Log("About to purchase " + item);
			_iap.purchase(item);
		}

      //  Global.ComfrimingBuy(item);
	}

	public void Refresh()
	{
		if (_iap != null)
		{
			Debug.Log("About to refresh");
			_iap.refresh();
		}
	}

	public void Restore()
	{
		if (_iap != null)
		{
			Debug.Log("About to restore");
			_iap.restore();
		}
	}

	//
	// Event Handlers
	//

	public void onInitialized(bool status)
	{
		Debug.Log("PurchaseHandler.onInitialized " + status);
        GetComponent<PurchaseManager>().OnIABInit(status);
	}

    public void onSuccess(Product product, string jsonString)
	{
        Debug.Log("PurchaseHandler.onSuccess: " + product.name + "  BuyID:" + product.receiptCipheredPayload);
      //  Global.photonService.ConfirmPurchase(jsonString);
	}

	public void onFailure(Product product, string message)
	{
		Debug.Log("PurchaseHandler.onFailure " + message);
	}

	public void onCanceled(Product product)
	{
		Debug.Log("PurchaseHandler.onCanceled product: " + product.name);
	}

	public void onRestored(Product product)
	{
		Debug.Log("PurchaseHandler.onRestored: " + product.name);
	}

	public void onProductRequestSuccess(Product[] products)
	{
        if (enabled)
        {
            Dictionary<string, object> dictProducts = new Dictionary<string, object>();
            string currencyCode = "";
            int i = -1;
            
            foreach (var p in products)
            {
                if (++i == 0) currencyCode = p.currencyCode;
                dictProducts.Add(p.name, p.price);
                Debug.Log("Product: " + p.name + " price: " + p.price + " currencyCode" + currencyCode);
            }

            if (GetComponentInChildren<PurchaseManager>())
                GetComponentInChildren<PurchaseManager>().OnProductRequest(dictProducts, products, currencyCode);
        }
	}

	public void onProductRequestFailure(string message)
	{
		Debug.Log("PurchaseHandler.onProductRequestFailure: " + message);
	}

	public void onRestoreComplete(string message)
	{
		Debug.Log("PurchaseHandler.onRestoreComplete: " + message);
	}
}
