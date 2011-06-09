using System;

namespace Stetic.Wrapper
{
	public class Alignment : Container
	{
		public HorizontalAlignments Horizontal {
			get {
				if (AlignLeft) {
					return HorizontalAlignments.AlignLeft;
				}
				if (AlignRight) {
					return HorizontalAlignments.AlignRight;
				}
				if (AlignCenter) {
					return HorizontalAlignments.AlignCenter;
				}
				return HorizontalAlignments.None;
			}
			set {
				switch (value) {
				case HorizontalAlignments.AlignLeft : AlignLeft = true; break;
				case HorizontalAlignments.AlignRight : AlignRight = true; break;
				case HorizontalAlignments.AlignCenter : AlignCenter = true; break;
				}
			}
		}
		
		public bool AlignLeft {
			get {
				return ((Gtk.Alignment)Wrapped).Xalign == 0;
			}
			set {
				((Gtk.Alignment)Wrapped).Xalign = 0;
			}
		}
		
		public bool AlignRight {
			get {
				return ((Gtk.Alignment)Wrapped).Xalign == 1;
			}
			set {
				((Gtk.Alignment)Wrapped).Xalign = 1;
			}
		}
		
		public bool AlignCenter {
			get {
				return ((Gtk.Alignment)Wrapped).Xalign == 0.5f;
			}
			set {
				((Gtk.Alignment)Wrapped).Xalign = 0.5f;
			}
		}
	}
}

