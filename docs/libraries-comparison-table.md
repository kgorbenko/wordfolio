# Frontend Libraries Comparison Table

Quick comparison of all evaluated libraries for routing, state management, and data querying.

---

## 🗺️ Routing Libraries

| Feature | React Router v7 | TanStack Router ⭐ | Wouter |
|---------|----------------|-------------------|---------|
| **Bundle Size** | ~14KB | ~10KB | ~1.5KB |
| **Type Safety** | ⚠️ Good | ✅ Excellent | ✅ Good |
| **Learning Curve** | Medium | Medium | Easy |
| **DevTools** | ✅ Yes | ✅ Excellent | ❌ No |
| **Nested Routing** | ✅ Excellent | ✅ Excellent | ⚠️ Basic |
| **Data Loaders** | ✅ Yes | ✅ Yes | ❌ No |
| **Community** | ✅ Very Large | ⚠️ Growing | ⚠️ Small |
| **npm Downloads/week** | ~20M | ~500K | ~100K |
| **GitHub Stars** | ~53K | ~8K | ~6K |
| **SSR Support** | ✅ Yes | ✅ Yes | ⚠️ Limited |
| **File-based Routing** | ❌ No | ✅ Optional | ❌ No |
| **Best For** | Enterprise apps | Modern TS apps | Simple apps |

**Recommendation**: **TanStack Router** - Best type safety and DX for TypeScript projects

---

## 🔄 State Management Libraries

| Feature | Zustand ⭐ | Redux Toolkit | Jotai | Context API |
|---------|-----------|---------------|-------|-------------|
| **Bundle Size** | ~1KB | ~15KB | ~3KB | 0KB (built-in) |
| **Boilerplate** | ✅ Minimal | ⚠️ Moderate | ✅ Minimal | ✅ Minimal |
| **Type Safety** | ✅ Excellent | ✅ Good | ✅ Excellent | ✅ Good |
| **Learning Curve** | Easy | Steep | Medium | Easy |
| **DevTools** | ✅ Yes | ✅ Excellent | ⚠️ Basic | ❌ No |
| **Performance** | ✅ Excellent | ✅ Good | ✅ Excellent | ❌ Poor |
| **Provider Hell** | ✅ None | ✅ None | ✅ None | ❌ Yes |
| **Testing** | ✅ Easy | ⚠️ Complex | ✅ Easy | ✅ Easy |
| **npm Downloads/week** | ~5M | ~8M | ~1M | N/A |
| **GitHub Stars** | ~48K | ~48K | ~18K | N/A |
| **Middleware** | ✅ Yes | ✅ Extensive | ✅ Yes | ❌ No |
| **Best For** | Most apps | Large apps | Complex state | Simple apps |

**Recommendation**: **Zustand** - Perfect balance of simplicity and power

---

## 📡 Data Querying Libraries

| Feature | TanStack Query ⭐ | SWR | RTK Query | Manual (fetch/axios) |
|---------|------------------|-----|-----------|----------------------|
| **Bundle Size** | ~15KB | ~5KB | ~20KB+ | ~0-5KB |
| **Caching** | ✅ Advanced | ✅ Good | ✅ Good | ❌ Manual |
| **Type Safety** | ✅ Excellent | ✅ Good | ✅ Good | ✅ Manual |
| **Learning Curve** | Medium | Easy | Steep | Easy |
| **DevTools** | ✅ Excellent | ⚠️ Basic | ✅ Good | ❌ No |
| **Features** | ✅ Extensive | ⚠️ Good | ✅ Good | ❌ None |
| **Dependencies** | None | None | Redux required | None |
| **Background Updates** | ✅ Yes | ✅ Yes | ✅ Yes | ❌ Manual |
| **Retries** | ✅ Auto | ✅ Auto | ✅ Auto | ❌ Manual |
| **Pagination** | ✅ Built-in | ✅ Built-in | ✅ Built-in | ❌ Manual |
| **Optimistic Updates** | ✅ Yes | ✅ Yes | ✅ Yes | ❌ Manual |
| **npm Downloads/week** | ~7M | ~2M | Included in RTK | N/A |
| **GitHub Stars** | ~42K | ~30K | Part of RTK | N/A |
| **Best For** | Most apps | Simple apps | Redux apps | Tiny apps |

**Recommendation**: **TanStack Query** - Industry standard with best features

---

## 📊 Overall Comparison

### Recommended Stack Total

| Aspect | Value |
|--------|-------|
| **Total Bundle Size** | ~26KB (minified+gzipped) |
| **Total npm Downloads** | ~12.5M/week combined |
| **Type Safety** | ✅ Excellent across all |
| **DevTools** | ✅ All have excellent tools |
| **Learning Curve** | ⚠️ Medium (but worth it) |
| **Community Support** | ✅ Strong and active |
| **Long-term Viability** | ✅ Excellent (TanStack + Zustand) |

### Alternative Stacks

#### Conservative Stack (Heavy but Proven)
```
React Router v7 + Redux Toolkit + RTK Query
Bundle: ~49KB | Community: Largest | DX: Good
```

#### Minimal Stack (Light but Limited)
```
Wouter + Context API + SWR
Bundle: ~6.5KB | Community: Small | DX: Basic
```

---

## 🎯 Decision Matrix

### When to Use Recommended Stack
- ✅ TypeScript project (Wordfolio ✓)
- ✅ React 19+ (Wordfolio ✓)
- ✅ Need type safety (Wordfolio ✓)
- ✅ Want great DX (Wordfolio ✓)
- ✅ Small to medium app (Wordfolio ✓)
- ✅ Modern architecture (Wordfolio ✓)

### When to Consider Alternatives

**Use Conservative Stack if:**
- Team already knows Redux well
- Very large enterprise app (100+ routes)
- Need strict architectural patterns
- Have Redux ecosystem dependencies

**Use Minimal Stack if:**
- Very simple app (< 10 routes)
- Bundle size is critical (< 50KB total)
- Prototyping/MVP
- No TypeScript

---

## 📈 Trend Analysis

### npm Download Trends (Last 12 Months)

**Growing:**
- TanStack Router: 📈 +300% (explosive growth)
- Zustand: 📈 +50%
- TanStack Query: 📈 +30%

**Stable:**
- React Router: ➡️ Stable at ~20M/week
- Redux Toolkit: ➡️ Stable at ~8M/week
- SWR: ➡️ Stable at ~2M/week

**This indicates:** Our recommended stack is gaining momentum while remaining stable.

---

## 🔐 Maintenance & Support

| Library | Maintainer | Last Major Release | Breaking Changes | Support |
|---------|-----------|-------------------|-----------------|----------|
| TanStack Router | Tanner Linsley (TanStack) | v1.0 (2024) | New lib | ✅ Active |
| TanStack Query | Tanner Linsley (TanStack) | v5.0 (2024) | Every major | ✅ Active |
| Zustand | Poimandres Collective | v4.5 (2024) | Minimal | ✅ Active |
| React Router | Remix/Shopify | v7.0 (2024) | Frequent | ✅ Active |
| Redux Toolkit | Redux Team | v2.0 (2024) | Major overhaul | ✅ Active |

**Verdict**: All recommended libraries have active maintenance and strong backing.

---

## 🎓 Learning Resources Availability

| Library | Documentation | Tutorials | Videos | Examples |
|---------|--------------|-----------|--------|----------|
| TanStack Router | ✅ Excellent | ⚠️ Growing | ⚠️ Few | ✅ Good |
| TanStack Query | ✅ Excellent | ✅ Many | ✅ Many | ✅ Excellent |
| Zustand | ✅ Good | ✅ Many | ✅ Many | ✅ Good |
| React Router | ✅ Excellent | ✅ Extensive | ✅ Extensive | ✅ Excellent |
| Redux Toolkit | ✅ Excellent | ✅ Extensive | ✅ Extensive | ✅ Excellent |

**Note**: While TanStack Router is newer, it shares concepts with React Router and has excellent docs.

---

## 💡 Summary

The recommended stack of **TanStack Router + Zustand + TanStack Query** wins on:

1. ✅ **Type Safety** - Best-in-class TypeScript support
2. ✅ **Developer Experience** - Excellent DevTools for all three
3. ✅ **Modern Architecture** - Built for React 19+
4. ✅ **Bundle Size** - Reasonable at ~26KB total
5. ✅ **Maintenance** - Active development and support
6. ✅ **Integration** - Work seamlessly together
7. ✅ **Performance** - Optimized for production

**Result**: Perfect fit for Wordfolio's modern TypeScript + React 19 + Vite stack.

---

**See Also:**
- [Full Research Document](./frontend-libraries-research.md)
- [Quick Decision Guide](./frontend-stack-decision.md)
