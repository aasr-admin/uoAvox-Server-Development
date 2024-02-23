These are the only edits needed to make ActionAI work. The rest is drag-and-drop.


////////////////
/*** FIRST ***/
//////////////

In BaseCreature.cs, find the method ChangeAIType

Between case AIType.AI_Predator & case AIType.AI_Thief:
add this new case

                #region ActionAI
				case AIType.AI_ActionAI:
					m_AI = new ActionAI(this);
					break;
				#endregion 

resulting code in ChangeAIType should look like this now

            switch ( NewAI )
			{
				case AIType.AI_Melee:
					m_AI = new MeleeAI(this);
					break;
				case AIType.AI_Animal:
					m_AI = new AnimalAI(this);
					break;
				case AIType.AI_Berserk:
					m_AI = new BerserkAI(this);
					break;
				case AIType.AI_Archer:
					m_AI = new ArcherAI(this);
					break;
				case AIType.AI_Healer:
					m_AI = new HealerAI(this);
					break;
				case AIType.AI_Vendor:
					m_AI = new VendorAI(this);
					break;
				case AIType.AI_Mage:
					m_AI = new MageAI(this);
					break;
				case AIType.AI_Predator:
					//m_AI = new PredatorAI(this);
					m_AI = new MeleeAI(this);
					break;
				
				#region ActionAI
				case AIType.AI_ActionAI:
					m_AI = new ActionAI(this);
					break;
				#endregion 
				
				case AIType.AI_Thief:
					m_AI = new ThiefAI(this);
					break;
			}
///////////////
/*** NEXT ***/
/////////////

In BaseAI.cs find the enum AIType and add the section below, again between AI.Predator and AI_Thief

        #region ActionAI
		AI_ActionAI,
		#endregion

Resulting code should look like this

	public enum AIType
	{
		AI_Use_Default,
		AI_Melee,
		AI_Animal,
		AI_Archer,
		AI_Healer,
		AI_Vendor,
		AI_Mage,
		AI_Berserk,
		AI_Predator,

		#region ActionAI
		AI_ActionAI,
		#endregion
		
		AI_Thief
		
	}