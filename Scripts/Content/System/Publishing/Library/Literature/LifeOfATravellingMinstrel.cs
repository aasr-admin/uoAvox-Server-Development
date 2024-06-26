﻿using Server.Engines.Publishing;

namespace Server.Items
{
	public class LifeOfATravellingMinstrel : BaseBook
	{
		public static readonly BookContent Content = new BookContent
			(
				"The Life of a Travelling Minstrel", "Sarah of Yew",
				new BookPageInfo
				(
					"  While 'tis true that",
					"the musician who",
					"seeketh only to make",
					"sweet music for",
					"herself and for",
					"others needs little",
					"more than some",
					"talent, and stern"
				),
				new BookPageInfo
				(
					"practice at the chosen",
					"instrument, those of",
					"us who seek the open",
					"road shall find indeed",
					"that a greater skill is",
					"required. Herein",
					"discover those secrets",
					"which I have learned"
				),
				new BookPageInfo
				(
					"over the years as an",
					"itinerant performer...",
					"  Once I was in",
					"Jhelom, and",
					"accidentally angered a",
					"bravo of some local",
					"repute, whose blade",
					"flickered all too"
				),
				new BookPageInfo
				(
					"eagerly near my",
					"slender neck (for I",
					"was young then).",
					"After various threats",
					"to \"ruin my pretty",
					"face\" this bravo",
					"grabbed my arm in a",
					"most unseemly"
				),
				new BookPageInfo
				(
					"fashion and tossed",
					"me into a barbaric",
					"enclosure locally",
					"entitled a dueling pit.",
					"My plaintive cries",
					"for help went",
					"unheeded by the",
					"guards, for the"
				),
				new BookPageInfo
				(
					"inhabitants of Jhelom",
					"are eager indeed to",
					"measure fighting",
					"prowess at any time!",
					"  What saved me was",
					"the ability to",
					"improvise a melody",
					"and tune that"
				),
				new BookPageInfo
				(
					"satirized the",
					"proceedings, and",
					"sufficiently angered",
					"an onlooker to prod",
					"him to coming to my",
					"defense. Once that",
					"fight was underway,",
					"I was able to make"
				),
				new BookPageInfo
				(
					"good my escape.",
					"Hence, I regard the",
					"ability to incite fights",
					"as indispensable to",
					"the prudent bard.",
					"  Upon another",
					"occasion, 'twas the",
					"obverse side of that"
				),
				new BookPageInfo
				(
					"coin which saved me,",
					"for I was being held",
					"prisoner by a",
					"particularly nasty",
					"band of ruffians who",
					"had seized me",
					"unawares from the",
					"road to Vesper."
				),
				new BookPageInfo
				(
					"  They had worked",
					"themselves into a",
					"frenzy and were",
					"ready to attack and I",
					"fear, tear me limb",
					"from limb, when I",
					"began to sing",
					"frantically, tapping"
				),
				new BookPageInfo
				(
					"my falled drum with",
					"my tied up feet. The",
					"melody developed into",
					"a soothing one, and",
					"the brigands slowly",
					"calmed down to the",
					"extent of apologizing,",
					"and they let me go!"
				),
				new BookPageInfo
				(
					"  A final example I",
					"would pray you grant",
					"your attention: once I",
					"was lost upon a large",
					"isle far to the east of",
					"the mainland, well",
					"beyond Serpent's",
					"Hold, where lava"
				),
				new BookPageInfo
				(
					"made its sluggish",
					"way across the",
					"surface landscape.",
					"And this accursed",
					"land was filled with",
					"vile beasts and",
					"cunning dragons.",
					"  I was being pursued"
				),
				new BookPageInfo
				(
					"by one of said fell",
					"dragons when I found",
					"myself trapped. I",
					"quickly skirted a",
					"bubbling pool of molten",
					"rock and attempted to",
					"hide.",
					"  The dragon scented"
				),
				new BookPageInfo
				(
					"me and was",
					"preparing to skirt the",
					"pool, when I began to",
					"play a lusty tune",
					"upon my lute that",
					"attracted its attention.",
					"Mesmerized and",
					"enticed by the"
				),
				new BookPageInfo
				(
					"melody, it stepped",
					"directly toward sme,",
					"and into the",
					"lava-where its foot",
					"was so burned that it",
					"quickly hopped away,",
					"undignified and",
					"annoyed."
				),
				new BookPageInfo
				(
					"  'Tis my fond hope",
					"that other travelling",
					"minstrels shall learn",
					"from my experiences",
					"and apply themselves",
					"to practicing these",
					"skills in order to",
					"preserve life and"
				),
				new BookPageInfo
				(
					"limb."
				)
			);

		public override BookContent DefaultContent => Content;

		[Constructable]
		public LifeOfATravellingMinstrel() : base(Utility.Random(0xFEF, 2), false)
		{
		}

		public LifeOfATravellingMinstrel(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}
}