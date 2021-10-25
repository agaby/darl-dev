// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslEdge.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using QuickGraph;
using System.ComponentModel;

namespace DaslLanguage
{
    /// <summary>
    /// Class DaslEdge.
    /// </summary>
    public class DaslEdge : Edge<DaslVertex>, INotifyPropertyChanged
    {
        /// <summary>
        /// The identifier
        /// </summary>
        private string id;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID


        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaslEdge"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public DaslEdge(string id, DaslVertex source, DaslVertex target)
            : base(source, target)
        {
            ID = id;
        }


        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="info">The information.</param>
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
