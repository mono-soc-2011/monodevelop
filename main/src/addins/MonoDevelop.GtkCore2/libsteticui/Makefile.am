ADDIN_BUILD = $(top_builddir)/build/AddIns/MonoDevelop.GtkCore
ASSEMBLY = $(ADDIN_BUILD)/libsteticui.dll
assemblydir = $(MD_ADDIN_DIR)/MonoDevelop.GtkCore
assembly_DATA = $(ASSEMBLY) $(ASSEMBLY).mdb $(build_dll_config)

FILES =

RES =

DEPS = ../../../../build/bin/Mono.Cecil.dll

REFS =  \
	$(GLADE_SHARP_LIBS) \
	$(GLIB_SHARP_LIBS) \
	$(GTK_SHARP_LIBS) \
	-pkg:monodevelop \
	-r:Mono.Posix \
	-r:System \
	-r:System.Core \
	-r:System.Runtime.Remoting \
	-r:System.Xml

$(ASSEMBLY): $(build_sources) $(build_resources) $(DEPS)
	mkdir -p $(ADDIN_BUILD)
	$(CSC) $(CSC_FLAGS) -debug -unsafe -main:Stetic.ApplicationBackend -out:$@ \
	$(build_resources:%=/resource:%) $(build_sources) $(REFS) $(build_deps)

dll_config = libsteticui.dll.config
build_dll_config = $(ADDIN_BUILD)/$(dll_config)

$(build_dll_config): $(srcdir)/$(dll_config)
	mkdir -p $(ADDIN_BUILD)
	cp $(srcdir)/$(dll_config) $@

EXTRA_DIST = $(FILES) $(RES) $(dll_config)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb

include $(top_srcdir)/Makefile.include

