#
# Common settings
#
CFLAGS = $(OPTS) -D_CRT_SECURE_NO_WARNINGS -D_SCL_SECURE_NO_WARNINGS
LFLAGS = -language charm++ -module CkMulticast -module completion
CHARM_DIR = ../libs/charm
TBB_DIR = ../libs/tbb
SHASH_DIR = ../libs/sparsehash
JSON_DIR = ../libs/json
FLATBUF_DIR = ../libs/flatbuffers
CATCH_DIR = ../libs/catch
COMMON_INC_DIRS = -I$(TBB_DIR)/include -I$(SHASH_DIR)/src -I$(JSON_DIR)/src -I$(FLATBUF_DIR)/include -I$(CATCH_DIR)/single_include
FLATC = $(FLATBUF_DIR)/bin/flatc

#
# Project files
#
BIN = core
RUN = charmrun
MSG_DIR = ../../network/messages
SRCS = $(wildcard *.cpp)
OBJS = $(patsubst %.cpp, %.o, $(SRCS))
DCLS = $(patsubst %.ci, %.decl.h, $(wildcard *.ci))
DEFS = $(patsubst %.ci, %.def.h, $(wildcard *.ci))
FBSS = $(patsubst $(MSG_DIR)/%.fbs, %_generated.h, $(wildcard $(MSG_DIR)/*.fbs))

#
# Debug build settings
#
DBG_DIR = debug
DBG_BIN = $(DBG_DIR)/$(BIN)
DBG_RUN = $(DBG_DIR)/$(RUN)
DBG_FBSS = $(addprefix $(DBG_DIR)/, $(FBSS))
DBG_DCLS = $(addprefix $(DBG_DIR)/, $(DCLS))
DBG_DEFS = $(addprefix $(DBG_DIR)/, $(DEFS))
DBG_OBJS = $(addprefix $(DBG_DIR)/, $(OBJS))
DBG_CHARMFLAGS = -no-optimize
DBG_CFLAGS = $(CFLAGS) -g -I$(DBG_DIR) $(COMMON_INC_DIRS)
DBG_LFLAGS = $(LFLAGS) -g -L$(TBB_DIR)/lib -ltbb_debug -ltbbmalloc_debug -ltbbmalloc_proxy_debug
DBG_CHARMC = $(CHARM_DIR)/net-debug/bin/charmc
DBG_DEPFILE = $(DBG_DIR)/$(*F)
DBG_DEPEND = touch $(DBG_DEPFILE).Td && makedepend $(DBG_CFLAGS) -p $(DBG_DIR)/ -f $(DBG_DEPFILE).Td $< >/dev/null 2>&1

#
# Release build settings
#
RLS_DIR = release
RLS_BIN = $(RLS_DIR)/$(BIN)
RLS_RUN = $(RLS_DIR)/$(RUN)
RLS_FBSS = $(addprefix $(RLS_DIR)/, $(FBSS))
RLS_DCLS = $(addprefix $(RLS_DIR)/, $(DCLS))
RLS_DEFS = $(addprefix $(RLS_DIR)/, $(DEFS))
RLS_OBJS = $(addprefix $(RLS_DIR)/, $(OBJS))
RLS_CHARMFLAGS = -optimize -production
RLS_CFLAGS = $(CFLAGS) -I$(RLS_DIR) $(COMMON_INC_DIRS)
RLS_LFLAGS = $(LFLAGS) -L$(TBB_DIR)/lib -ltbb -ltbbmalloc -ltbbmalloc_proxy
RLS_CHARMC = $(CHARM_DIR)/net-release/bin/charmc
RLS_DEPFILE = $(RLS_DIR)/$(*F)
RLS_DEPEND = touch $(RLS_DEPFILE).Td && makedepend $(RLS_CFLAGS) -f $(RLS_DEPFILE).Td $< >/dev/null 2>&1

.PHONY: all clean test
.PHONY: debug-prep debug-build debug-clean debug-rebuild
.PHONY: release-prep release-build release-clean release-rebuild

#
# Default build
#
all: release-build

clean:
	rm -r -f $(DBG_DIR) $(RLS_DIR)

#
# Debug rules
#
debug-build: debug-prep $(DBG_BIN)

debug-prep:
	@mkdir -p $(DBG_DIR)

debug-clean:
	rm -f $(DBG_FBSS) $(DBG_DCLS) $(DBG_DEFS) $(DBG_OBJS) $(DBG_BIN) $(DBG_RUN)

debug-rebuild: debug-clean debug-build

test: debug-build
	$(DBG_BIN) --test

$(DBG_BIN): $(DBG_FBSS) $(DBG_DCLS) $(DBG_DEFS) $(DBG_OBJS)
	$(DBG_CHARMC) $(DBG_LFLAGS) -o $@ $(DBG_OBJS)
	@mv -f $(RUN) $(DBG_RUN)
	@cp -f $(TBB_DIR)/bin/tbb_debug.* $(DBG_DIR)
	@cp -f $(TBB_DIR)/bin/tbbmalloc_debug.* $(DBG_DIR)
	@cp -f $(TBB_DIR)/bin/tbbmalloc_proxy_debug.* $(DBG_DIR)

$(DBG_DIR)/%_generated.h: $(MSG_DIR)/%.fbs
	cd $(DBG_DIR) && ../$(FLATC) --cpp ../$<

$(DBG_DIR)/%.decl.h: %.ci
	cd $(DBG_DIR) && ../$(DBG_CHARMC) $(DBG_CHARMFLAGS) $(DBG_CFLAGS) ../$<

$(DBG_DIR)/%.def.h: %.ci
	cd $(DBG_DIR) && ../$(DBG_CHARMC) $(DBG_CHARMFLAGS) $(DBG_CFLAGS) ../$<

$(DBG_DIR)/%.o: %.cpp
	@$(DBG_DEPEND); \
	  cp $(DBG_DEPFILE).Td $(DBG_DEPFILE).d; \
	  sed -e 's/#.*//' -e 's/^[^:]*: *//' -e 's/ *\\$$//' \
		  -e '/^$$/ d' -e 's/$$/ :/' < $(DBG_DEPFILE).Td >> $(DBG_DEPFILE).d; \
	  rm -f $(DBG_DEPFILE).Td
	$(DBG_CHARMC) $(DBG_CHARMFLAGS) $(DBG_CFLAGS) -o $@ $<

-include $(SRCS:%.cpp=$(DBG_DIR)/%.d)

#
# Release rules
#
release-build: release-prep $(RLS_BIN)

release-prep:
	@mkdir -p $(RLS_DIR)

release-clean:
	rm -f $(RLS_FBSS) $(RLS_DCLS) $(RLS_DEFS) $(RLS_OBJS) $(RLS_BIN) $(RLS_RUN)

release-rebuild: release-clean release-build

$(RLS_BIN): $(RLS_FBSS) $(RLS_DCLS) $(RLS_DEFS) $(RLS_OBJS)
	$(RLS_CHARMC) $(RLS_LFLAGS) -o $@ $(RLS_OBJS)
	@mv -f $(RUN) $(RLS_RUN)
	@cp -f $(TBB_DIR)/bin/tbb.* $(RLS_DIR)
	@cp -f $(TBB_DIR)/bin/tbbmalloc.* $(RLS_DIR)
	@cp -f $(TBB_DIR)/bin/tbbmalloc_proxy.* $(RLS_DIR)

$(RLS_DIR)/%_generated.h: $(MSG_DIR)/%.fbs
	cd $(RLS_DIR) && ../$(FLATC) --cpp ../$<

$(RLS_DIR)/%.decl.h: %.ci
	cd $(RLS_DIR) && ../$(RLS_CHARMC) $(RLS_CHARMFLAGS) $(RLS_CFLAGS) ../$<

$(RLS_DIR)/%.def.h: %.ci
	cd $(RLS_DIR) && ../$(RLS_CHARMC) $(RLS_CHARMFLAGS) $(RLS_CFLAGS) ../$<

$(RLS_DIR)/%.o: %.cpp
	@$(RLS_DEPEND); \
	  cp $(RLS_DEPFILE).Td $(RLS_DEPFILE).d; \
	  sed -e 's/#.*//' -e 's/^[^:]*: *//' -e 's/ *\\$$//' \
		  -e '/^$$/ d' -e 's/$$/ :/' < $(RLS_DEPFILE).Td >> $(RLS_DEPFILE).d; \
	  rm -f $(RLS_DEPFILE).Td
	$(RLS_CHARMC) $(RLS_CHARMFLAGS) $(RLS_CFLAGS) -o $@ $<

-include $(SRCS:%.cpp=$(RLS_DIR)/%.d)
