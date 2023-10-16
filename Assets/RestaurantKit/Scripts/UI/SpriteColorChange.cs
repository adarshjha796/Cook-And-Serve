using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorChange : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	public void OnMouseOver()
	{
		spriteRenderer.color = new Color(.9f, .9f, .9f, 1);
	}

	public void OnMouseExit()
	{
		spriteRenderer.color = new Color(/*.8f, .8f, .2f, 1*/255 / 255, 255 / 255, 255 / 255, 1);
	}
}
