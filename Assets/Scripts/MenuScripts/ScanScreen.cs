using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScanScreen : StationScreen
{
	public BasicWeapon weaponToScan;

	public Image weaponIcon;
	public TextMeshProUGUI weaponNameText;
	public TextMeshProUGUI weaponStatsText;
	public TextMeshProUGUI weaponDescription;

	private static string weaponStatsFormat = "DAMAGE:\\t {0}\r\nDURABILITY: {1}";
	private static string weaponChargedFormat = "DAMAGE:\\t {0}\r\nCHARGED DAMAGE: {1}\r\nDURABILITY: {2}";

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		if (station.playerActor != null)
		{
			GameObject weaponObject = station.playerActor.getEquippedWeapon();
			if (weaponObject != null)
			{
				weaponToScan = weaponObject.GetComponentInChildren<BasicWeapon>();
			}
		}

		if (weaponToScan != null && weaponToScan._weaponScriptable.description != null && weaponToScan._weaponScriptable.description.Length > 0)
		{
			string statsText = "";
			if (weaponToScan.GetType() == typeof(SwingBatteryWeapon))
			{
				statsText = string.Format(weaponChargedFormat, weaponToScan._weaponScriptable.damage, ((SwingBatteryWeapon)weaponToScan).chargedDamage, weaponToScan._weaponScriptable.durability);
			}
			else
			{
				statsText = string.Format(weaponStatsFormat, weaponToScan._weaponScriptable.damage, weaponToScan._weaponScriptable.durability);
			}
			if (statsText.Length > 0)
			{
				weaponStatsText.text = statsText;
			}
			weaponNameText.text = weaponToScan._weaponScriptable.displayName;
			weaponDescription.text = weaponToScan._weaponScriptable.description;
			if (null != weaponToScan._weaponScriptable.icon)
			{
				weaponIcon.sprite = weaponToScan._weaponScriptable.icon;
			}
		}
		else
		{
			weaponNameText.text = "!ERROR!";
		}
	}
}