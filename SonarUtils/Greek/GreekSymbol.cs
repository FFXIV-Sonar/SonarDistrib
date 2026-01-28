namespace SonarUtils.Greek
{
    /// <summary>Greek symbols.</summary>
    public enum GreekSymbol : byte
    {
        Invalid,

        /// <summary>Αα.</summary>
        [GreekSymbol("Αα")]
        Alpha,

        /// <summary>Ββ.</summary>
        [GreekSymbol("Ββ")]
        Beta,

        /// <summary>Γγ.</summary>
        [GreekSymbol("Γγ")]
        Gamma,

        /// <summary>Δδ.</summary>
        [GreekSymbol("Δδ")]
        Delta,

        /// <summary>Εε.</summary>
        [GreekSymbol("Εε")]
        Epsilon,

        /// <summary>Ζζ.</summary>
        [GreekSymbol("Ζζ")]
        Zeta,

        /// <summary>Ηη.</summary>
        [GreekSymbol("Ηη")]
        Eta,

        /// <summary>Θθ.</summary>
        [GreekSymbol("Θθ")]
        Theta,

        /// <summary>Ιι.</summary>
        [GreekSymbol("Ιι")]
        Iota,

        /// <summary>Κκ.</summary>
        [GreekSymbol("Κκ")]
        Kappa,

        /// <summary>Λλ.</summary>
        [GreekSymbol("Λλ")]
        Lambda,

        /// <summary>Μμ.</summary>
        [GreekSymbol("Μμ")]
        Mu,

        /// <summary>Νν.</summary>
        [GreekSymbol("Νν")]
        Nu,

        /// <summary>Ξξ.</summary>
        [GreekSymbol("Ξξ")]
        Xi,

        /// <summary>Οο.</summary>
        [GreekSymbol("Οο")]
        Omicron,

        /// <summary>Ππ.</summary>
        [GreekSymbol("Ππ")]
        Pi,

        /// <summary>Ρρ.</summary>
        [GreekSymbol("Ρρ")]
        Rho,

        /// <summary>Σσ.</summary>
        /// <remarks><c>σ</c> is used generally while <c>ς</c> is used at the end of words. <see cref="GreekSymbolExtensions.get_LowerChar(GreekSymbol)"/> and <see cref="GreekSymbolExtensions.get_LowerString(GreekSymbol)"/> uses <c>σ</c></remarks>
        [GreekSymbol("Σσς")]
        Sigma,

        /// <summary>Ττ.</summary>
        [GreekSymbol("Ττ")]
        Tau,

        /// <summary>Υυ.</summary>
        [GreekSymbol("Υυ")]
        Upsilon,

        /// <summary>Φφ.</summary>
        [GreekSymbol("Φφ")]
        Phi,

        /// <summary>Χχ.</summary>
        [GreekSymbol("Χχ")]
        Chi,

        /// <summary>Ψψ.</summary>
        [GreekSymbol("Ψψ")]
        Psi,

        /// <summary>Ωω.</summary>
        [GreekSymbol("Ωω")]
        Omega,
    }
}
