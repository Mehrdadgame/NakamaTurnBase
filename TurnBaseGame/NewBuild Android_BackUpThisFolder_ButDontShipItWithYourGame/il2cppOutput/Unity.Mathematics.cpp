#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>
#include <stdint.h>



struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
struct IFormatProvider_tC202922D43BFF3525109ABF3FB79625F5646AB52;
struct Il2CppEagerStaticClassConstructionAttribute_t01F60DE85CC3427802A892466A27D5245AEA1BAF;
struct String_t;

IL2CPP_EXTERN_C RuntimeClass* ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA;

struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

struct U3CModuleU3E_t9A99FA938A0D410D25805C4E4D63978454163723 
{
};
struct Il2CppArrayBounds;

struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};

struct String_t  : public RuntimeObject
{
	int32_t ____stringLength_4;
	Il2CppChar ____firstChar_5;
};

struct String_t_StaticFields
{
	String_t* ___Empty_6;
};

struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};

struct math_t084A7021C0897F87DB209393BE7FBDFD6A7FBFA6  : public RuntimeObject
{
};

struct DebuggerProxy_t3A8A5A2398C653D7A869D7B07919E08EB23696E8  : public RuntimeObject
{
};

struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value_0;
};

struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString_5;
	String_t* ___FalseString_6;
};

struct Double_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F 
{
	double ___m_value_0;
};

struct Il2CppEagerStaticClassConstructionAttribute_t01F60DE85CC3427802A892466A27D5245AEA1BAF  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};

struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value_0;
};

struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	int64_t ___m_value_0;
};

struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value_0;
};

struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};

struct uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 
{
	uint32_t ___x_0;
	uint32_t ___y_1;
	uint32_t ___z_2;
	uint32_t ___w_3;
};

struct LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___longValue_0;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___longValue_0_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___doubleValue_1;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___doubleValue_1_forAlignmentOnly;
		};
	};
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t math_lzcnt_m121BDDDEE89F5A401E2E5F0AD900D22E47C8741C_inline (uint32_t ___x0, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t math_tzcnt_m07FD7550AAB5D94312E99571B112D652E8230360_inline (uint32_t ___x0, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 math_uint4_m7F6A5341327B9EF6B80C82FD1B93E4F32287A336_inline (uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 uint4_op_Multiply_mDD93D0730642A1089848321B9B0E5E923EE575ED_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___lhs0, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs1, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t math_csum_m6A99E69A84442A729781A97F78B260223DD01D8F_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___x0, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs0, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool uint4_Equals_m689E5D21501C5846BF031E4864E8DBB46F467386 (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, RuntimeObject* ___o0, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t math_hash_m1A4778A79FFB5E05B04BD09B0F85EA9483D8A3CA_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___v0, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t uint4_GetHashCode_m195FED91BE8D7CCE0039A8DE6B6B0BB849FBCC5A_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_m918500C1EFB475181349A79989BB79BB36102894 (String_t* ___format0, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___args1, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* uint4_ToString_mEF61205FE152AAB74331B24080C347AA829E435B_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE (uint32_t* __this, String_t* ___format0, RuntimeObject* ___provider1, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* uint4_ToString_m3350471364AB1D6033E088C0DF789376954921E3_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, String_t* ___format0, RuntimeObject* ___formatProvider1, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Il2CppEagerStaticClassConstructionAttribute__ctor_mCACE94326399F3059F318EB568BD8E45037E3139 (Il2CppEagerStaticClassConstructionAttribute_t01F60DE85CC3427802A892466A27D5245AEA1BAF* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_min_m02D43DF516544C279AF660EA4731449C82991849 (int32_t ___x0, int32_t ___y1, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___x0;
		int32_t L_1 = ___y1;
		if ((((int32_t)L_0) < ((int32_t)L_1)))
		{
			goto IL_0006;
		}
	}
	{
		int32_t L_2 = ___y1;
		return L_2;
	}

IL_0006:
	{
		int32_t L_3 = ___x0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t math_min_mA22BCFB62A81B533821704D26BE23D8D6402C8EB (int64_t ___x0, int64_t ___y1, const RuntimeMethod* method) 
{
	{
		int64_t L_0 = ___x0;
		int64_t L_1 = ___y1;
		if ((((int64_t)L_0) < ((int64_t)L_1)))
		{
			goto IL_0006;
		}
	}
	{
		int64_t L_2 = ___y1;
		return L_2;
	}

IL_0006:
	{
		int64_t L_3 = ___x0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_max_m9083201D37A8ED0157B127B5878D9B7F3A2A40BE (int32_t ___x0, int32_t ___y1, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___x0;
		int32_t L_1 = ___y1;
		if ((((int32_t)L_0) > ((int32_t)L_1)))
		{
			goto IL_0006;
		}
	}
	{
		int32_t L_2 = ___y1;
		return L_2;
	}

IL_0006:
	{
		int32_t L_3 = ___x0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t math_max_mEBAE1BF7FA6B43BD0F4AE2E47FB6190041F8CE43 (int64_t ___x0, int64_t ___y1, const RuntimeMethod* method) 
{
	{
		int64_t L_0 = ___x0;
		int64_t L_1 = ___y1;
		if ((((int64_t)L_0) > ((int64_t)L_1)))
		{
			goto IL_0006;
		}
	}
	{
		int64_t L_2 = ___y1;
		return L_2;
	}

IL_0006:
	{
		int64_t L_3 = ___x0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_lzcnt_mA6B7E71DB1B5D4CE8B67C66FF1AC4339FA368D07 (int32_t ___x0, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___x0;
		int32_t L_1;
		L_1 = math_lzcnt_m121BDDDEE89F5A401E2E5F0AD900D22E47C8741C_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_lzcnt_m121BDDDEE89F5A401E2E5F0AD900D22E47C8741C (uint32_t ___x0, const RuntimeMethod* method) 
{
	LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		uint32_t L_0 = ___x0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		return ((int32_t)32);
	}

IL_0006:
	{
		(&V_0)->___doubleValue_1 = (0.0);
		uint32_t L_1 = ___x0;
		(&V_0)->___longValue_0 = ((int64_t)il2cpp_codegen_add(((int64_t)4841369599423283200LL), ((int64_t)(uint64_t)L_1)));
		double* L_2 = (&(&V_0)->___doubleValue_1);
		double* L_3 = L_2;
		double L_4 = *((double*)L_3);
		*((double*)L_3) = (double)((double)il2cpp_codegen_subtract(L_4, (4503599627370496.0)));
		LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA L_5 = V_0;
		int64_t L_6 = L_5.___longValue_0;
		return ((int32_t)il2cpp_codegen_subtract(((int32_t)1054), ((int32_t)((int64_t)(L_6>>((int32_t)52))))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_tzcnt_m85FEAD596A8E327F7B6820310B7FBD9822BA735C (int32_t ___x0, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___x0;
		int32_t L_1;
		L_1 = math_tzcnt_m07FD7550AAB5D94312E99571B112D652E8230360_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t math_tzcnt_m07FD7550AAB5D94312E99571B112D652E8230360 (uint32_t ___x0, const RuntimeMethod* method) 
{
	LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		uint32_t L_0 = ___x0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		return ((int32_t)32);
	}

IL_0006:
	{
		uint32_t L_1 = ___x0;
		uint32_t L_2 = ___x0;
		___x0 = ((int32_t)((int32_t)L_1&((int32_t)(uint32_t)((-((int64_t)(uint64_t)L_2))))));
		(&V_0)->___doubleValue_1 = (0.0);
		uint32_t L_3 = ___x0;
		(&V_0)->___longValue_0 = ((int64_t)il2cpp_codegen_add(((int64_t)4841369599423283200LL), ((int64_t)(uint64_t)L_3)));
		double* L_4 = (&(&V_0)->___doubleValue_1);
		double* L_5 = L_4;
		double L_6 = *((double*)L_5);
		*((double*)L_5) = (double)((double)il2cpp_codegen_subtract(L_6, (4503599627370496.0)));
		LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA L_7 = V_0;
		int64_t L_8 = L_7.___longValue_0;
		return ((int32_t)il2cpp_codegen_subtract(((int32_t)((int64_t)(L_8>>((int32_t)52)))), ((int32_t)1023)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C (int64_t ___x0, const RuntimeMethod* method) 
{
	{
		int64_t L_0 = ___x0;
		___x0 = ((int64_t)il2cpp_codegen_subtract(L_0, ((int64_t)1)));
		int64_t L_1 = ___x0;
		int64_t L_2 = ___x0;
		___x0 = ((int64_t)(L_1|((int64_t)(L_2>>1))));
		int64_t L_3 = ___x0;
		int64_t L_4 = ___x0;
		___x0 = ((int64_t)(L_3|((int64_t)(L_4>>2))));
		int64_t L_5 = ___x0;
		int64_t L_6 = ___x0;
		___x0 = ((int64_t)(L_5|((int64_t)(L_6>>4))));
		int64_t L_7 = ___x0;
		int64_t L_8 = ___x0;
		___x0 = ((int64_t)(L_7|((int64_t)(L_8>>8))));
		int64_t L_9 = ___x0;
		int64_t L_10 = ___x0;
		___x0 = ((int64_t)(L_9|((int64_t)(L_10>>((int32_t)16)))));
		int64_t L_11 = ___x0;
		int64_t L_12 = ___x0;
		___x0 = ((int64_t)(L_11|((int64_t)(L_12>>((int32_t)32)))));
		int64_t L_13 = ___x0;
		return ((int64_t)il2cpp_codegen_add(L_13, ((int64_t)1)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t math_csum_m6A99E69A84442A729781A97F78B260223DD01D8F (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___x0, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___x0;
		uint32_t L_1 = L_0.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2 = ___x0;
		uint32_t L_3 = L_2.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___x0;
		uint32_t L_5 = L_4.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_6 = ___x0;
		uint32_t L_7 = L_6.___w_3;
		return ((int32_t)il2cpp_codegen_add(((int32_t)il2cpp_codegen_add(((int32_t)il2cpp_codegen_add((int32_t)L_1, (int32_t)L_3)), (int32_t)L_5)), (int32_t)L_7));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 math_uint4_m7F6A5341327B9EF6B80C82FD1B93E4F32287A336 (uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = ___x0;
		uint32_t L_1 = ___y1;
		uint32_t L_2 = ___z2;
		uint32_t L_3 = ___w3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4;
		memset((&L_4), 0, sizeof(L_4));
		uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline((&L_4), L_0, L_1, L_2, L_3, NULL);
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t math_hash_m1A4778A79FFB5E05B04BD09B0F85EA9483D8A3CA (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___v0, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___v0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_1;
		L_1 = math_uint4_m7F6A5341327B9EF6B80C82FD1B93E4F32287A336_inline(((int32_t)-1265451243), ((int32_t)-747495197), ((int32_t)2057487037), ((int32_t)-513029987), NULL);
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2;
		L_2 = uint4_op_Multiply_mDD93D0730642A1089848321B9B0E5E923EE575ED_inline(L_0, L_1, NULL);
		uint32_t L_3;
		L_3 = math_csum_m6A99E69A84442A729781A97F78B260223DD01D8F_inline(L_2, NULL);
		return ((int32_t)il2cpp_codegen_add((int32_t)L_3, ((int32_t)2057338067)));
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008 (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = ___x0;
		__this->___x_0 = L_0;
		uint32_t L_1 = ___y1;
		__this->___y_1 = L_1;
		uint32_t L_2 = ___z2;
		__this->___z_2 = L_2;
		uint32_t L_3 = ___w3;
		__this->___w_3 = L_3;
		return;
	}
}
IL2CPP_EXTERN_C  void uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_AdjustorThunk (RuntimeObject* __this, uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline(_thisAdjusted, ___x0, ___y1, ___z2, ___w3, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 uint4_op_Multiply_mDD93D0730642A1089848321B9B0E5E923EE575ED (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___lhs0, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs1, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___lhs0;
		uint32_t L_1 = L_0.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2 = ___rhs1;
		uint32_t L_3 = L_2.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___lhs0;
		uint32_t L_5 = L_4.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_6 = ___rhs1;
		uint32_t L_7 = L_6.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_8 = ___lhs0;
		uint32_t L_9 = L_8.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_10 = ___rhs1;
		uint32_t L_11 = L_10.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_12 = ___lhs0;
		uint32_t L_13 = L_12.___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_14 = ___rhs1;
		uint32_t L_15 = L_14.___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_16;
		memset((&L_16), 0, sizeof(L_16));
		uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline((&L_16), ((int32_t)il2cpp_codegen_multiply((int32_t)L_1, (int32_t)L_3)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_5, (int32_t)L_7)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_9, (int32_t)L_11)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_13, (int32_t)L_15)), NULL);
		return L_16;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs0, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = __this->___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_1 = ___rhs0;
		uint32_t L_2 = L_1.___x_0;
		if ((!(((uint32_t)L_0) == ((uint32_t)L_2))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_3 = __this->___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___rhs0;
		uint32_t L_5 = L_4.___y_1;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_5))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_6 = __this->___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_7 = ___rhs0;
		uint32_t L_8 = L_7.___z_2;
		if ((!(((uint32_t)L_6) == ((uint32_t)L_8))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_9 = __this->___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_10 = ___rhs0;
		uint32_t L_11 = L_10.___w_3;
		return (bool)((((int32_t)L_9) == ((int32_t)L_11))? 1 : 0);
	}

IL_0039:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C  bool uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB_AdjustorThunk (RuntimeObject* __this, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs0, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	bool _returnValue;
	_returnValue = uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB_inline(_thisAdjusted, ___rhs0, method);
	return _returnValue;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool uint4_Equals_m689E5D21501C5846BF031E4864E8DBB46F467386 (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, RuntimeObject* ___o0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		RuntimeObject* L_0 = ___o0;
		if (!((RuntimeObject*)IsInstSealed((RuntimeObject*)L_0, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9_il2cpp_TypeInfo_var)))
		{
			goto IL_0017;
		}
	}
	{
		RuntimeObject* L_1 = ___o0;
		V_0 = ((*(uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*)((uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*)(uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*)UnBox(L_1, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9_il2cpp_TypeInfo_var))));
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2 = V_0;
		bool L_3;
		L_3 = uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB_inline(__this, L_2, NULL);
		return L_3;
	}

IL_0017:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C  bool uint4_Equals_m689E5D21501C5846BF031E4864E8DBB46F467386_AdjustorThunk (RuntimeObject* __this, RuntimeObject* ___o0, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	bool _returnValue;
	_returnValue = uint4_Equals_m689E5D21501C5846BF031E4864E8DBB46F467386(_thisAdjusted, ___o0, method);
	return _returnValue;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t uint4_GetHashCode_m195FED91BE8D7CCE0039A8DE6B6B0BB849FBCC5A (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = (*(uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*)__this);
		uint32_t L_1;
		L_1 = math_hash_m1A4778A79FFB5E05B04BD09B0F85EA9483D8A3CA_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C  int32_t uint4_GetHashCode_m195FED91BE8D7CCE0039A8DE6B6B0BB849FBCC5A_AdjustorThunk (RuntimeObject* __this, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	int32_t _returnValue;
	_returnValue = uint4_GetHashCode_m195FED91BE8D7CCE0039A8DE6B6B0BB849FBCC5A_inline(_thisAdjusted, method);
	return _returnValue;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* uint4_ToString_mEF61205FE152AAB74331B24080C347AA829E435B (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA);
		s_Il2CppMethodInitialized = true;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = L_0;
		uint32_t L_2 = __this->___x_0;
		uint32_t L_3 = L_2;
		RuntimeObject* L_4 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_3);
		NullCheck(L_1);
		ArrayElementTypeCheck (L_1, L_4);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_5 = L_1;
		uint32_t L_6 = __this->___y_1;
		uint32_t L_7 = L_6;
		RuntimeObject* L_8 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_7);
		NullCheck(L_5);
		ArrayElementTypeCheck (L_5, L_8);
		(L_5)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_8);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_9 = L_5;
		uint32_t L_10 = __this->___z_2;
		uint32_t L_11 = L_10;
		RuntimeObject* L_12 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_11);
		NullCheck(L_9);
		ArrayElementTypeCheck (L_9, L_12);
		(L_9)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_12);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_13 = L_9;
		uint32_t L_14 = __this->___w_3;
		uint32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_15);
		NullCheck(L_13);
		ArrayElementTypeCheck (L_13, L_16);
		(L_13)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_16);
		String_t* L_17;
		L_17 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA, L_13, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C  String_t* uint4_ToString_mEF61205FE152AAB74331B24080C347AA829E435B_AdjustorThunk (RuntimeObject* __this, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	String_t* _returnValue;
	_returnValue = uint4_ToString_mEF61205FE152AAB74331B24080C347AA829E435B_inline(_thisAdjusted, method);
	return _returnValue;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* uint4_ToString_m3350471364AB1D6033E088C0DF789376954921E3 (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, String_t* ___format0, RuntimeObject* ___formatProvider1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA);
		s_Il2CppMethodInitialized = true;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = L_0;
		uint32_t* L_2 = (&__this->___x_0);
		String_t* L_3 = ___format0;
		RuntimeObject* L_4 = ___formatProvider1;
		String_t* L_5;
		L_5 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_2, L_3, L_4, NULL);
		NullCheck(L_1);
		ArrayElementTypeCheck (L_1, L_5);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_5);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = L_1;
		uint32_t* L_7 = (&__this->___y_1);
		String_t* L_8 = ___format0;
		RuntimeObject* L_9 = ___formatProvider1;
		String_t* L_10;
		L_10 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_7, L_8, L_9, NULL);
		NullCheck(L_6);
		ArrayElementTypeCheck (L_6, L_10);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_10);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_11 = L_6;
		uint32_t* L_12 = (&__this->___z_2);
		String_t* L_13 = ___format0;
		RuntimeObject* L_14 = ___formatProvider1;
		String_t* L_15;
		L_15 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_12, L_13, L_14, NULL);
		NullCheck(L_11);
		ArrayElementTypeCheck (L_11, L_15);
		(L_11)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_15);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_16 = L_11;
		uint32_t* L_17 = (&__this->___w_3);
		String_t* L_18 = ___format0;
		RuntimeObject* L_19 = ___formatProvider1;
		String_t* L_20;
		L_20 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_17, L_18, L_19, NULL);
		NullCheck(L_16);
		ArrayElementTypeCheck (L_16, L_20);
		(L_16)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_20);
		String_t* L_21;
		L_21 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA, L_16, NULL);
		return L_21;
	}
}
IL2CPP_EXTERN_C  String_t* uint4_ToString_m3350471364AB1D6033E088C0DF789376954921E3_AdjustorThunk (RuntimeObject* __this, String_t* ___format0, RuntimeObject* ___formatProvider1, const RuntimeMethod* method)
{
	uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*>(__this + _offset);
	String_t* _returnValue;
	_returnValue = uint4_ToString_m3350471364AB1D6033E088C0DF789376954921E3_inline(_thisAdjusted, ___format0, ___formatProvider1, method);
	return _returnValue;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t math_lzcnt_m121BDDDEE89F5A401E2E5F0AD900D22E47C8741C_inline (uint32_t ___x0, const RuntimeMethod* method) 
{
	LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		uint32_t L_0 = ___x0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		return ((int32_t)32);
	}

IL_0006:
	{
		(&V_0)->___doubleValue_1 = (0.0);
		uint32_t L_1 = ___x0;
		(&V_0)->___longValue_0 = ((int64_t)il2cpp_codegen_add(((int64_t)4841369599423283200LL), ((int64_t)(uint64_t)L_1)));
		double* L_2 = (&(&V_0)->___doubleValue_1);
		double* L_3 = L_2;
		double L_4 = *((double*)L_3);
		*((double*)L_3) = (double)((double)il2cpp_codegen_subtract(L_4, (4503599627370496.0)));
		LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA L_5 = V_0;
		int64_t L_6 = L_5.___longValue_0;
		return ((int32_t)il2cpp_codegen_subtract(((int32_t)1054), ((int32_t)((int64_t)(L_6>>((int32_t)52))))));
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t math_tzcnt_m07FD7550AAB5D94312E99571B112D652E8230360_inline (uint32_t ___x0, const RuntimeMethod* method) 
{
	LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		uint32_t L_0 = ___x0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		return ((int32_t)32);
	}

IL_0006:
	{
		uint32_t L_1 = ___x0;
		uint32_t L_2 = ___x0;
		___x0 = ((int32_t)((int32_t)L_1&((int32_t)(uint32_t)((-((int64_t)(uint64_t)L_2))))));
		(&V_0)->___doubleValue_1 = (0.0);
		uint32_t L_3 = ___x0;
		(&V_0)->___longValue_0 = ((int64_t)il2cpp_codegen_add(((int64_t)4841369599423283200LL), ((int64_t)(uint64_t)L_3)));
		double* L_4 = (&(&V_0)->___doubleValue_1);
		double* L_5 = L_4;
		double L_6 = *((double*)L_5);
		*((double*)L_5) = (double)((double)il2cpp_codegen_subtract(L_6, (4503599627370496.0)));
		LongDoubleUnion_tD71C400B6C4CD1A7F13CE8125AC6BBC7A22791CA L_7 = V_0;
		int64_t L_8 = L_7.___longValue_0;
		return ((int32_t)il2cpp_codegen_subtract(((int32_t)((int64_t)(L_8>>((int32_t)52)))), ((int32_t)1023)));
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = ___x0;
		__this->___x_0 = L_0;
		uint32_t L_1 = ___y1;
		__this->___y_1 = L_1;
		uint32_t L_2 = ___z2;
		__this->___z_2 = L_2;
		uint32_t L_3 = ___w3;
		__this->___w_3 = L_3;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 math_uint4_m7F6A5341327B9EF6B80C82FD1B93E4F32287A336_inline (uint32_t ___x0, uint32_t ___y1, uint32_t ___z2, uint32_t ___w3, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = ___x0;
		uint32_t L_1 = ___y1;
		uint32_t L_2 = ___z2;
		uint32_t L_3 = ___w3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4;
		memset((&L_4), 0, sizeof(L_4));
		uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline((&L_4), L_0, L_1, L_2, L_3, NULL);
		return L_4;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 uint4_op_Multiply_mDD93D0730642A1089848321B9B0E5E923EE575ED_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___lhs0, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs1, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___lhs0;
		uint32_t L_1 = L_0.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2 = ___rhs1;
		uint32_t L_3 = L_2.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___lhs0;
		uint32_t L_5 = L_4.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_6 = ___rhs1;
		uint32_t L_7 = L_6.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_8 = ___lhs0;
		uint32_t L_9 = L_8.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_10 = ___rhs1;
		uint32_t L_11 = L_10.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_12 = ___lhs0;
		uint32_t L_13 = L_12.___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_14 = ___rhs1;
		uint32_t L_15 = L_14.___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_16;
		memset((&L_16), 0, sizeof(L_16));
		uint4__ctor_m59B6A219A0285C60FCF2977679BF89C72B502008_inline((&L_16), ((int32_t)il2cpp_codegen_multiply((int32_t)L_1, (int32_t)L_3)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_5, (int32_t)L_7)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_9, (int32_t)L_11)), ((int32_t)il2cpp_codegen_multiply((int32_t)L_13, (int32_t)L_15)), NULL);
		return L_16;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t math_csum_m6A99E69A84442A729781A97F78B260223DD01D8F_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___x0, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___x0;
		uint32_t L_1 = L_0.___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2 = ___x0;
		uint32_t L_3 = L_2.___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___x0;
		uint32_t L_5 = L_4.___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_6 = ___x0;
		uint32_t L_7 = L_6.___w_3;
		return ((int32_t)il2cpp_codegen_add(((int32_t)il2cpp_codegen_add(((int32_t)il2cpp_codegen_add((int32_t)L_1, (int32_t)L_3)), (int32_t)L_5)), (int32_t)L_7));
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool uint4_Equals_mAA88D674B2411C0A7D6C25AA596320EF79B58DDB_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___rhs0, const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = __this->___x_0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_1 = ___rhs0;
		uint32_t L_2 = L_1.___x_0;
		if ((!(((uint32_t)L_0) == ((uint32_t)L_2))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_3 = __this->___y_1;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_4 = ___rhs0;
		uint32_t L_5 = L_4.___y_1;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_5))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_6 = __this->___z_2;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_7 = ___rhs0;
		uint32_t L_8 = L_7.___z_2;
		if ((!(((uint32_t)L_6) == ((uint32_t)L_8))))
		{
			goto IL_0039;
		}
	}
	{
		uint32_t L_9 = __this->___w_3;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_10 = ___rhs0;
		uint32_t L_11 = L_10.___w_3;
		return (bool)((((int32_t)L_9) == ((int32_t)L_11))? 1 : 0);
	}

IL_0039:
	{
		return (bool)0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t math_hash_m1A4778A79FFB5E05B04BD09B0F85EA9483D8A3CA_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 ___v0, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = ___v0;
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_1;
		L_1 = math_uint4_m7F6A5341327B9EF6B80C82FD1B93E4F32287A336_inline(((int32_t)-1265451243), ((int32_t)-747495197), ((int32_t)2057487037), ((int32_t)-513029987), NULL);
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_2;
		L_2 = uint4_op_Multiply_mDD93D0730642A1089848321B9B0E5E923EE575ED_inline(L_0, L_1, NULL);
		uint32_t L_3;
		L_3 = math_csum_m6A99E69A84442A729781A97F78B260223DD01D8F_inline(L_2, NULL);
		return ((int32_t)il2cpp_codegen_add((int32_t)L_3, ((int32_t)2057338067)));
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t uint4_GetHashCode_m195FED91BE8D7CCE0039A8DE6B6B0BB849FBCC5A_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) 
{
	{
		uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9 L_0 = (*(uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9*)__this);
		uint32_t L_1;
		L_1 = math_hash_m1A4778A79FFB5E05B04BD09B0F85EA9483D8A3CA_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* uint4_ToString_mEF61205FE152AAB74331B24080C347AA829E435B_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA);
		s_Il2CppMethodInitialized = true;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = L_0;
		uint32_t L_2 = __this->___x_0;
		uint32_t L_3 = L_2;
		RuntimeObject* L_4 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_3);
		NullCheck(L_1);
		ArrayElementTypeCheck (L_1, L_4);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_5 = L_1;
		uint32_t L_6 = __this->___y_1;
		uint32_t L_7 = L_6;
		RuntimeObject* L_8 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_7);
		NullCheck(L_5);
		ArrayElementTypeCheck (L_5, L_8);
		(L_5)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_8);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_9 = L_5;
		uint32_t L_10 = __this->___z_2;
		uint32_t L_11 = L_10;
		RuntimeObject* L_12 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_11);
		NullCheck(L_9);
		ArrayElementTypeCheck (L_9, L_12);
		(L_9)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_12);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_13 = L_9;
		uint32_t L_14 = __this->___w_3;
		uint32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_il2cpp_TypeInfo_var, &L_15);
		NullCheck(L_13);
		ArrayElementTypeCheck (L_13, L_16);
		(L_13)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_16);
		String_t* L_17;
		L_17 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA, L_13, NULL);
		return L_17;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* uint4_ToString_m3350471364AB1D6033E088C0DF789376954921E3_inline (uint4_t6C69CBFAE9BF0F727D52B68779D4A3F0DBA8D5C9* __this, String_t* ___format0, RuntimeObject* ___formatProvider1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA);
		s_Il2CppMethodInitialized = true;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = L_0;
		uint32_t* L_2 = (&__this->___x_0);
		String_t* L_3 = ___format0;
		RuntimeObject* L_4 = ___formatProvider1;
		String_t* L_5;
		L_5 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_2, L_3, L_4, NULL);
		NullCheck(L_1);
		ArrayElementTypeCheck (L_1, L_5);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_5);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = L_1;
		uint32_t* L_7 = (&__this->___y_1);
		String_t* L_8 = ___format0;
		RuntimeObject* L_9 = ___formatProvider1;
		String_t* L_10;
		L_10 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_7, L_8, L_9, NULL);
		NullCheck(L_6);
		ArrayElementTypeCheck (L_6, L_10);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_10);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_11 = L_6;
		uint32_t* L_12 = (&__this->___z_2);
		String_t* L_13 = ___format0;
		RuntimeObject* L_14 = ___formatProvider1;
		String_t* L_15;
		L_15 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_12, L_13, L_14, NULL);
		NullCheck(L_11);
		ArrayElementTypeCheck (L_11, L_15);
		(L_11)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_15);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_16 = L_11;
		uint32_t* L_17 = (&__this->___w_3);
		String_t* L_18 = ___format0;
		RuntimeObject* L_19 = ___formatProvider1;
		String_t* L_20;
		L_20 = UInt32_ToString_m464396B0FE2115F3CEA38AEECDDB0FACC3AADADE(L_17, L_18, L_19, NULL);
		NullCheck(L_16);
		ArrayElementTypeCheck (L_16, L_20);
		(L_16)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_20);
		String_t* L_21;
		L_21 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(_stringLiteralE134155080D59FDE2A83A55D0BB359039175A4EA, L_16, NULL);
		return L_21;
	}
}
